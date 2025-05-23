using Application.Enum;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class TransferHandler
    {
        private readonly ITransferRepos _transferRepos;
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IImportRepos _importRepos;
        private readonly IStoreExportRepos _storeExportRepos;
        private readonly IImportStoreRepos _importStoreRepos;
        private readonly IAuditLogRepository _auditLogRepos;
        private readonly IWareHousesStockRepository _stockRepos;
        private readonly IMapper _mapper;


        public TransferHandler(
            ITransferRepos transferRepos,
            IDispatchRepos dispatchRepos,
            IImportRepos importRepos,
            IStoreExportRepos storeExportRepos,
            IImportStoreRepos importStoreRepos,
            IAuditLogRepository auditLogRepos,
            IWareHousesStockRepository stockRepos,
            IMapper mapper)
        {
            _transferRepos = transferRepos;
            _dispatchRepos = dispatchRepos;
            _importRepos = importRepos;
            _storeExportRepos = storeExportRepos;
            _importStoreRepos = importStoreRepos;
            _auditLogRepos = auditLogRepos;
            _stockRepos = stockRepos;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<TransferFullFlowDto>> CreateTransferFullFlowAsync(CreateTransferFullFlowDto request)
        {
            // 1. Map và chuẩn bị Transfer
            var newTransfer = _mapper.Map<Transfer>(request);
            newTransfer.CreatedDate = DateTime.Now;
            newTransfer.Status = "Approved";
            foreach (var detailDto in request.TransferDetails)
            {
                var newDetail = _mapper.Map<TransferDetail>(detailDto);
                newTransfer.TransferDetails.Add(newDetail);
            }

            // 2. Tạo Dispatch (có validation tồn kho)
            Dispatch newDispatch;
            try
            {
                newDispatch = await CreateDispatchAsync(request, newTransfer);
            }
            catch (InvalidOperationException ex)
            {
                return new ResponseDTO<TransferFullFlowDto>(null, false, ex.Message);
            }

            // 3. Tạo Import
            var newImport = await CreateImportAsync(request, newTransfer);

            // 4. Gán ID và lưu Transfer
            newTransfer.DispatchId = newDispatch.DispatchId;
            newTransfer.ImportId = newImport.ImportId;
            _transferRepos.Add(newTransfer);
            await _transferRepos.SaveChangesAsync();

            // Audit cho Transfer và TransferDetails
            await LogAuditAsync("Transfer", "CREATE", newTransfer.TransferOrderId, request.CreatedBy, newTransfer, "Tạo mới đơn chuyển hàng");
            foreach (var d in newTransfer.TransferDetails)
            {
                await LogAuditAsync("TransferDetail", "CREATE", d.TransferOrderDetailId, request.CreatedBy, d, "Tạo chi tiết đơn chuyển hàng");
            }

            // 5. Tạo StoreExport
            await CreateStoreExportRecordsAsync(request, newDispatch);

            // 6. Trả về kết quả
            var resultDto = _mapper.Map<TransferFullFlowDto>(newTransfer);
            return new ResponseDTO<TransferFullFlowDto>(resultDto, true, "Tạo đơn chuyển hàng và các đơn liên quan thành công!");
        }

        private async Task<Dispatch> CreateDispatchAsync(CreateTransferFullFlowDto request, Transfer transfer)
        {
            // Validation tồn kho
            int sourceWarehouseId = request.SourceWarehouseId;
            foreach (var td in transfer.TransferDetails)
            {
                var stock = await _stockRepos.GetByWarehouseAndVariantAsync(sourceWarehouseId, td.VariantId);
                if (stock == null)
                    throw new InvalidOperationException(
                        $"Sản phẩm VariantId={td.VariantId} không tồn tại trong kho #{sourceWarehouseId}.");
                if (stock.StockQuantity < td.Quantity)
                    throw new InvalidOperationException(
                        $"Kho #{sourceWarehouseId} chỉ còn {stock.StockQuantity} sản phẩm VariantId={td.VariantId}, không đủ để xuất {td.Quantity}.");
            }

            // Tạo Dispatch và DispatchDetails
            var dispatch = new Dispatch
            {
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.Now,
                Status = "Approved",
                ReferenceNumber = !string.IsNullOrEmpty(request.DispatchReferenceNumber) && request.DispatchReferenceNumber.StartsWith("DIS")
                                  ? request.DispatchReferenceNumber
                                  : "DIS" + new Random().Next(100, 1000),
                Remarks = "Tự động tạo từ chuyển hàng"
            };

            foreach (var td in transfer.TransferDetails)
            {
                dispatch.DispatchDetails.Add(new DispatchDetail
                {
                    VariantId = td.VariantId,
                    Quantity = td.Quantity
                });
            }

            _dispatchRepos.Add(dispatch);
            await _dispatchRepos.SaveChangesAsync();

            // Audit
            await LogAuditAsync("Dispatch", "CREATE", dispatch.DispatchId, request.CreatedBy, dispatch, "Tạo mới đơn xuất hàng");
            foreach (var d in dispatch.DispatchDetails)
            {
                await LogAuditAsync("DispatchDetail", "CREATE", d.DispatchDetailId, request.CreatedBy, d, "Tạo chi tiết đơn xuất hàng");
            }

            return dispatch;
        }

        private async Task<Import> CreateImportAsync(CreateTransferFullFlowDto request, Transfer transfer)
        {
            decimal totalCost = request.TransferDetails.Sum(d => d.Quantity * d.CostPrice);

            var import = new Import
            {
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.Now,
                Status = "Approved",
                ReferenceNumber = !string.IsNullOrEmpty(request.ImportReferenceNumber) && request.ImportReferenceNumber.StartsWith("IIN")
                                    ? request.ImportReferenceNumber
                                    : "IIN" + new Random().Next(100, 1000),
                TotalCost = totalCost,
                ApprovedDate = null,
                CompletedDate = null
            };

            foreach (var td in transfer.TransferDetails)
            {
                var detail = new ImportDetail
                {
                    ProductVariantId = td.VariantId,
                    Quantity = td.Quantity,
                    CostPrice = 0,
                    Import = import
                };
                import.ImportDetails.Add(detail);
            }

            _importRepos.Add(import);
            await _importRepos.SaveChangesAsync();

            var warehouse = await _importRepos.GetWareHouseByIdAsync(request.DestinationWarehouseId);
            int? handleBy = warehouse?.ShopManagerId;

            foreach (var detail in import.ImportDetails)
            {
                var store = new ImportStoreDetail
                {
                    AllocatedQuantity = detail.Quantity,
                    Status = "Pending",
                    Comments = "Nhập hàng vào warehouse đích cho chuyển hàng",
                    ImportDetail = detail,
                    WarehouseId = request.DestinationWarehouseId,
                    HandleBy = handleBy
                };
                _importStoreRepos.Add(store);
                await _importStoreRepos.SaveChangesAsync();
                await LogAuditAsync("ImportStoreDetail", "CREATE", store.ImportStoreId, request.CreatedBy, store, "Tạo bản ghi nhập hàng");
            }

            await LogAuditAsync("Import", "CREATE", import.ImportId, request.CreatedBy, import, "Tạo mới đơn nhập hàng");
            foreach (var d in import.ImportDetails)
                await LogAuditAsync("ImportDetail", "CREATE", d.ImportDetailId, request.CreatedBy, d, "Tạo chi tiết đơn nhập hàng");

            return import;
        }

        private async Task CreateStoreExportRecordsAsync(CreateTransferFullFlowDto request, Dispatch dispatch)
        {
            var storeExports = new List<StoreExportStoreDetail>();
            var warehouse = await _importRepos.GetWareHouseByIdAsync(request.SourceWarehouseId);
            var handleBy = warehouse?.ShopManagerId;

            foreach (var dd in dispatch.DispatchDetails)
            {
                var store = new StoreExportStoreDetail
                {
                    DispatchDetailId = dd.DispatchDetailId,
                    WarehouseId = request.SourceWarehouseId,
                    AllocatedQuantity = dd.Quantity,
                    Status = "Pending",
                    Comments = "Xuất hàng từ warehouse nguồn cho chuyển hàng",
                    HandleBy = handleBy
                };
                _storeExportRepos.Add(store);
                storeExports.Add(store);
            }

            await _storeExportRepos.SaveChangesAsync();
            foreach (var s in storeExports)
                await LogAuditAsync("StoreExportStoreDetail", "CREATE", s.DispatchStoreDetailId, request.CreatedBy, s, "Tạo bản ghi xuất hàng");
        }

        private async Task LogAuditAsync(string tableName, string operation, int recordId, int changedBy, object entity, string comment)
        {
            var audit = new AuditLog
            {
                TableName = tableName,
                RecordId = recordId.ToString(),
                Operation = operation,
                ChangeDate = DateTime.Now,
                ChangedBy = changedBy,
                ChangeData = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                Comment = comment
            };
            _auditLogRepos.Add(audit);
            await _auditLogRepos.SaveChangesAsync();
        }

        public async Task<Transfer> GetTransferByIdAsync(int transferId)
        {
            // Ví dụ: sử dụng repository để truy xuất theo id
            var transfer = await _transferRepos.GetByIdWithDetailsAsync(transferId);
            return transfer;
        }

        /// <summary>
        /// Lấy thông tin đơn xuất kho (Dispatch/Export) dựa trên TransferId.
        /// Giả sử trong hệ thống, đơn xuất kho được liên kết với Transfer thông qua trường TransferId.
        /// </summary>
        /// <param name="transferId">Id của đơn chuyển hàng liên quan.</param>
        /// <returns>Đối tượng Dispatch (Export) hoặc null nếu không tìm thấy.</returns>
        public async Task<Dispatch> GetExportByTransferIdAsync(int transferId)
        {
            // Ví dụ: giả sử repository có phương thức truy xuất theo TransferId
            var export = await _dispatchRepos.GetDispatchByTransferIdAsync(transferId);
            return export;
        }

        /// <summary>
        /// Lấy thông tin đơn nhập kho (Import) dựa trên TransferId.
        /// Giả sử trong hệ thống, đơn nhập kho được liên kết với Transfer qua TransferId.
        /// </summary>
        /// <param name="transferId">Id của đơn chuyển hàng liên quan.</param>
        /// <returns>Đối tượng Import hoặc null nếu không tìm thấy.</returns>
        public async Task<Import> GetImportByTransferIdAsync(int transferId)
        {
            // Ví dụ: sử dụng repository cho Import để lấy theo TransferId
            var import = await _importRepos.GetImportByTransferIdAsync(transferId);
            return import;
        }
        //Duc Anh

        public async Task<JSONTransferDispatchImportGet> GetJSONTransferById(int id)
        {
            var data = await _transferRepos.GetJSONTransferOrderById(id);
            var data2 = await _dispatchRepos.GetJSONDispatchById(data.DispatchId);

            if (data == null)
            {
                throw new Exception("Transfer does not exsist!");
            }
            var jsonTransfer = _mapper.Map<JSONTransferOrderDTO>(data);
            var jsonImport = _mapper.Map<JSONImportDTO>(data.Import);
            var jsonDispatch = _mapper.Map<JSONDispatchDTO>(data2);

            var audit = await _auditLogRepos.GetAuditLogsByTableAndRecordIdAsync(TableEnumEXE.Transfer.ToString(), id.ToString());
            var jsonAuditLogs = _mapper.Map<List<AuditLogRes>>(audit);

            return new JSONTransferDispatchImportGet
            {
                JSONTransfer = jsonTransfer,
                JSONImport = jsonImport,
                JSONDispatch = jsonDispatch,
                AuditLogs = jsonAuditLogs
            };
        }
    }
}