using Application.DTO.Response;
using Application.Services;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateImportHandler
    {
        private readonly IProductVarRepos _productVar;

        private readonly IImportRepos _impRepos;
        private readonly IAuditLogRepository _auditLogRepos;
        private readonly IWarehouseRepository _warehouseRepo;
        private readonly IWareHousesStockRepository _stockRepo;
        private readonly IWarehouseStaffRepos _wsRepos;
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IDispatchDetailRepository _dispatchDetailRepo;
        private readonly ITransferRepos _transferRepo;
        private readonly ITransferDetailRepository _transferDetailRepo;
        private readonly IImportStoreRepos _importStoreRepo;
        private readonly IStoreExportRepos _storeExportRepo;
        private readonly IMapper _mapper;
        private readonly Random _rng = new();
        private readonly ReportService _reportService;


        public CreateImportHandler(
            IImportRepos importRepos,
            IAuditLogRepository auditLogRepos,
            IWarehouseRepository warehouseRepo,
            IWareHousesStockRepository stockRepo,
            IWarehouseStaffRepos wsRepos,
            IDispatchRepos dispatchRepos,
            IDispatchDetailRepository dispatchDetailRepo,
            ITransferRepos transferRepo,
            ITransferDetailRepository transferDetailRepo,
            IImportStoreRepos importStoreRepo,
            IStoreExportRepos storeExportRepo,
            ReportService reportService,
            IProductVarRepos productVar,
            IMapper mapper)
        {
            _impRepos = importRepos;
            _auditLogRepos = auditLogRepos;
            _warehouseRepo = warehouseRepo;
            _stockRepo = stockRepo;
            _wsRepos = wsRepos;
            _dispatchRepos = dispatchRepos;
            _dispatchDetailRepo = dispatchDetailRepo;
            _transferRepo = transferRepo;
            _transferDetailRepo = transferDetailRepo;
            _importStoreRepo = importStoreRepo;
            _storeExportRepo = storeExportRepo;
            _reportService = reportService;
            _mapper = mapper;
            _productVar = productVar;
        }
        public async Task<ResponseDTO<Import>> CreatePurchaseImportAsync(PurchaseImportCreateDto dto)
        {
            try
            {
                // 1. Validate DTO
                if (dto.ImportDetails == null || !dto.ImportDetails.Any())
                    return new ResponseDTO<Import>(null, false, "Phải có ít nhất 1 sản phẩm.");
                if (dto.ImportDetails.Any(d => d.CostPrice <= 0))
                    return new ResponseDTO<Import>(null, false, "CostPrice phải lớn hơn 0.");

                // 2. Tạo Import + ImportDetails
                var importEntity = new Import
                {
                    CreatedBy = dto.CreatedBy,
                    ApprovedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Status = "Approved",
                    ImportType = "Purchase",
                    ReferenceNumber = GenerateReferenceNumber(),
                    ImportDetails = dto.ImportDetails.Select(d => new ImportDetail
                    {
                        ProductVariantId = d.ProductVariantId,
                        Quantity = d.Quantity,
                        CostPrice = d.CostPrice
                    }).ToList()
                };
                importEntity.TotalCost = importEntity.ImportDetails.Sum(x => x.Quantity * x.CostPrice);

                // 3. Lưu Import vào DB
                await _impRepos.AddAsync(importEntity);
                await _impRepos.SaveChangesAsync();

                // 4. Ghi AuditLog cho Import vừa tạo
                var serializedImport = JsonConvert.SerializeObject(importEntity,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                var auditLog = new AuditLog
                {
                    TableName = "Import",
                    RecordId = importEntity.ImportId.ToString(),
                    Operation = "CREATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = importEntity.CreatedBy,
                    ChangeData = serializedImport,
                    Comment = "Tạo mới đơn import Purchase"
                };
                await _auditLogRepos.AddAsync(auditLog);
                await _auditLogRepos.SaveChangesAsync();

                // 5. Lấy kho tổng của owner
                var warehouse = await _warehouseRepo.GetOwnerWarehouseAsync();
                if (warehouse == null)
                    return new ResponseDTO<Import>(null, false, "Không tìm thấy kho tổng của owner.");

                // 6. Lấy danh sách Checker
                var allCheckers = await _wsRepos.GetByWarehouseAndRoleAsync(warehouse.WarehouseId, "Checker");
                var unused = string.IsNullOrWhiteSpace(warehouse.UnusedCheckerIds)
                    ? allCheckers.Select(w => w.StaffDetailId).ToList()
                    : warehouse.UnusedCheckerIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                if (!unused.Any())
                    unused = allCheckers.Select(w => w.StaffDetailId).ToList();

                int chosenStaffId = unused[_rng.Next(unused.Count)];
                unused.Remove(chosenStaffId);

                var storeDetails = new List<ImportStoreDetail>();
                foreach (var det in importEntity.ImportDetails)
                {
                    storeDetails.Add(new ImportStoreDetail
                    {
                        ImportDetailId = det.ImportDetailId,
                        WarehouseId = warehouse.WarehouseId,
                        AllocatedQuantity = det.Quantity,
                        Status = "Processing",
                        Comments = "Đơn Nhập Hàng Tự Động bởi hệ thống",
                        StaffDetailId = chosenStaffId,
                        HandleBy = null
                    });
                }

                // 8. Lưu ImportStoreDetails
                await _importStoreRepo.AddRangeAsync(storeDetails);
                await _impRepos.SaveChangesAsync();  // hoặc SaveChanges của importStoreRepos

                // 9. Ghi AuditLog cho từng ImportStoreDetail
                foreach (var sd in storeDetails)
                {
                    var serializedSd = JsonConvert.SerializeObject(sd,
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    var auditLogSd = new AuditLog
                    {
                        TableName = "ImportStoreDetail",
                        RecordId = sd.ImportStoreId.ToString(),   // EF đã set ra ID sau SaveChanges
                        Operation = "CREATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = importEntity.CreatedBy,
                        ChangeData = serializedSd,
                        Comment = "Tạo mới ImportStoreDetail cho ImportId " + importEntity.ImportId
                    };
                    await _auditLogRepos.AddAsync(auditLogSd);
                }
                await _auditLogRepos.SaveChangesAsync();

                // 10. Cập nhật lại danh sách unused vào Warehouse
                warehouse.UnusedCheckerIds = string.Join(",", unused);
                await _warehouseRepo.UpdateAsync(warehouse);

                return new ResponseDTO<Import>(importEntity, true,
                    "Tạo đơn Purchase thành công, gán Checker và ghi AuditLog cho Import + StoreDetails.");
            }
            catch (Exception ex)
            {
                return new ResponseDTO<Import>(null, false, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        private string GenerateReferenceNumber()
            => $"IMP-PUR-{DateTime.Now:yyyyMMddHHmmss}";

        public async Task<ResponseDTO<Import>> CreatePurchaseImportFromExcelAsync(IFormFile file, int createdBy)
        {
            if (file == null || file.Length == 0)
                return new ResponseDTO<Import>(null, false, "Vui lòng chọn file Excel.");

            var details = new List<PurchaseImportDetailDto>();
            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var sheet = workbook.Worksheet(1);
                var firstRow = sheet.FirstRowUsed();
                if (firstRow == null)
                    return new ResponseDTO<Import>(null, false, "File Excel không có dữ liệu.");

                int row = firstRow.RowNumber() + 1;
                while (true)
                {
                    var skuCell = sheet.Cell(row, 1);
                    if (skuCell.IsEmpty()) break;

                    string sku = skuCell.GetString().Trim();
                    var variant = await _productVar.GetBySkuAsync(sku);
                    if (variant == null)
                        return new ResponseDTO<Import>(null, false, $"Dòng {row}: Không tìm thấy variant với SKU '{sku}'.");

                    int qty = sheet.Cell(row, 2).GetValue<int>();
                    decimal cost = sheet.Cell(row, 3).GetValue<decimal>();

                    if (qty <= 0 || cost <= 0)
                        return new ResponseDTO<Import>(null, false, $"Dòng {row}: Quantity và CostPrice phải > 0.");

                    details.Add(new PurchaseImportDetailDto
                    {
                        ProductVariantId = variant.VariantId,
                        Quantity = qty,
                        CostPrice = cost
                    });

                    row++;
                }
            }

            if (!details.Any())
                return new ResponseDTO<Import>(null, false, "File Excel không có dòng dữ liệu hợp lệ.");

            var dto = new PurchaseImportCreateDto
            {
                CreatedBy = createdBy,
                ImportDetails = details
            };

            return await CreatePurchaseImportAsync(dto);
        }
        public async Task<ResponseDTO<ImportSupplementReportDto>> CreateSupplementImportAsync(SupplementImportRequestDto request)
        {
            // Validate OriginalImportId
            if (request.OriginalImportId <= 0)
                return new ResponseDTO<ImportSupplementReportDto>(null, false, "OriginalImportId không hợp lệ.");

            // Load original import
            var oldImport = await _impRepos.GetByIdAsync(request.OriginalImportId);
            if (oldImport == null)
                return new ResponseDTO<ImportSupplementReportDto>(null, false, "Đơn gốc không tồn tại.");

            // Branch on the actual ImportType from database, trimmed to ignore whitespace
            var importType = oldImport.ImportType?.Trim();

            if (string.Equals(importType, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return await HandlePurchaseSupplementAsync(request);
            }
            else if (string.Equals(importType, "Transfer", StringComparison.OrdinalIgnoreCase))
            {
                return await HandleTransferSupplementAsync(request);
            }
            else
            {
                return new ResponseDTO<ImportSupplementReportDto>(null, false, $"ImportType '{oldImport.ImportType}' không hợp lệ.");
            }
        }

        public async Task<ResponseDTO<ImportSupplementReportDto>> HandlePurchaseSupplementAsync(SupplementImportRequestDto request)
        {
            try
            {
                // Kiểm tra đầu vào
                if (request.OriginalImportId <= 0)
                {
                    Console.WriteLine("OriginalImportId không hợp lệ.");
                    throw new ArgumentException("OriginalImportId không hợp lệ.");
                }
                if (request.ImportDetails == null || !request.ImportDetails.Any())
                {
                    Console.WriteLine("Không có thông tin import detail.");
                    throw new ArgumentException("Không có thông tin import detail.");
                }

                // Lấy đơn cũ với eager loading các chi tiết
                var oldImport = await _impRepos.GetByIdAsyncWithDetails(request.OriginalImportId);
                if (oldImport == null)
                {
                    Console.WriteLine("Đơn cũ không tồn tại.");
                    throw new ArgumentException("Đơn cũ không tồn tại.");
                }

                // Map request sang entity Import (đơn bổ sung)
                var newImport = _mapper.Map<Import>(request);
                newImport.CreatedBy = oldImport.CreatedBy;
                newImport.ReferenceNumber = oldImport.ReferenceNumber;
                newImport.CreatedDate = DateTime.Now;
                newImport.Status = "Approved";
                newImport.ApprovedDate = DateTime.Now;
                newImport.CompletedDate = null;
                newImport.OriginalImportId = oldImport.ImportId;
                newImport.ImportType = "Supplement";

                decimal totalCost = 0;
                newImport.ImportDetails = new List<ImportDetail>();

                // Duyệt qua từng ImportDetail của đơn cũ để tạo mới
                foreach (var oldDetail in oldImport.ImportDetails)
                {
                    var missingStores = oldDetail.ImportStoreDetails?
                        .Where(s => (s.ActualReceivedQuantity ?? 0) < s.AllocatedQuantity)
                        .ToList();

                    if (missingStores == null || !missingStores.Any())
                    {
                        Console.WriteLine($"Không còn store thiếu cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId}. Bỏ qua bước này.");
                        continue;
                    }

                    var reqDetail = request.ImportDetails.FirstOrDefault(d => d.ProductVariantId == oldDetail.ProductVariantId);
                    if (reqDetail == null)
                    {
                        Console.WriteLine($"Thiếu thông tin unitPrice cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId}");
                        throw new ArgumentException($"Thiếu thông tin unitPrice cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId}");
                    }

                    int totalMissing = 0;
                    var newDetail = new ImportDetail
                    {
                        ProductVariantId = oldDetail.ProductVariantId,
                        Quantity = 0, // sẽ cập nhật sau
                        CostPrice = 0,
                        ImportStoreDetails = new List<ImportStoreDetail>()
                    };

                    // Duyệt qua các store thiếu để tạo mới ImportStoreDetail
                    foreach (var store in missingStores)
                    {
                        int actualReceived = store.ActualReceivedQuantity ?? 0;
                        int missing = store.AllocatedQuantity - actualReceived;
                        totalMissing += missing;

                        newDetail.ImportStoreDetails.Add(new ImportStoreDetail
                        {
                            ActualReceivedQuantity = null,
                            AllocatedQuantity = missing,
                            Status = "Processing",
                            StaffDetailId = store.StaffDetailId,
                            WarehouseId = store.WarehouseId,
                            HandleBy = store.HandleBy

                        });
                    }

                    newDetail.Quantity = totalMissing;
                    if (totalMissing > 0)
                    {
                        newImport.ImportDetails.Add(newDetail);
                        totalCost += totalMissing * reqDetail.CostPrice;
                        Console.WriteLine($"Đã tạo ImportDetail cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId} với số lượng thiếu = {totalMissing}.");

                        // Cập nhật trạng thái của các ImportStoreDetails cũ thành "Handled"
                        foreach (var store in missingStores)
                        {
                            store.Status = "Handled";
                        }
                    }
                }

                newImport.TotalCost = totalCost;

                // Lưu đơn bổ sung mới qua repository
                _impRepos.Add(newImport);
                await _impRepos.SaveChangesAsync();
                Console.WriteLine("Lưu đơn bổ sung mới thành công qua repository.");

                // Tạo AuditLog cho đơn mới
                var serializedNewImport = JsonConvert.SerializeObject(newImport,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                var auditLogNew = new AuditLog
                {
                    TableName = "Import",
                    RecordId = newImport.ImportId.ToString(),
                    Operation = "CREATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = oldImport.CreatedBy,
                    ChangeData = serializedNewImport,
                    Comment = "Tạo mới đơn import bổ sung dựa trên đơn cũ"
                };
                _auditLogRepos.Add(auditLogNew);
                await _auditLogRepos.SaveChangesAsync();
                Console.WriteLine("AuditLog cho đơn mới đã được tạo thành công.");

                // Cập nhật trạng thái cho đơn cũ
                oldImport.Status = "Supplement Created";
                await _impRepos.SaveChangesAsync();
                Console.WriteLine("Cập nhật trạng thái của đơn cũ thành 'Supplement Created' thành công.");

                var serializedOldImport = JsonConvert.SerializeObject(oldImport,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                var auditLogOld = new AuditLog
                {
                    TableName = "Import",
                    RecordId = oldImport.ImportId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = oldImport.CreatedBy,
                    ChangeData = serializedOldImport,
                    Comment = "Cập nhật đơn cũ với status 'Supplement Created'"
                };
                _auditLogRepos.Add(auditLogOld);
                await _auditLogRepos.SaveChangesAsync();
                Console.WriteLine("AuditLog cho đơn cũ đã được tạo thành công.");

                // Map entity sang DTO response
                var resultDto = _mapper.Map<ImportDto>(newImport);
                Console.WriteLine("Mapping từ entity sang DTO response thành công.");

                // Tải lại đầy đủ dữ liệu (nếu cần) để tạo báo cáo
                var supplementImportEntity = await _impRepos.GetByIdAsync(newImport.ImportId);
                var oldImportEntity = await _impRepos.GetByIdAsync(oldImport.ImportId);
                if (supplementImportEntity == null || oldImportEntity == null)
                {
                    Console.WriteLine("Không tìm thấy dữ liệu đơn nhập khi tạo báo cáo.");
                    throw new Exception("Không tìm thấy dữ liệu đơn nhập khi tạo báo cáo.");
                }

                // Gọi ReportService để tạo báo cáo nhập bổ sung
                byte[] reportFileBytes = _reportService.GenerateImportSupplementSlip(supplementImportEntity, oldImportEntity);
                string reportBase64 = Convert.ToBase64String(reportFileBytes);

                // Tạo DTO kết hợp ImportDto và chuỗi báo cáo
                var resultWithReport = new ImportSupplementReportDto
                {
                    ImportData = resultDto,
                    ReportFileBase64 = reportBase64
                };

                return new ResponseDTO<ImportSupplementReportDto>(resultWithReport, true, "Tạo đơn import bổ sung thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Quá trình tạo import bổ sung thất bại: {ex.Message}");
                return new ResponseDTO<ImportSupplementReportDto>(null, false, $"Quá trình tạo import bổ sung thất bại: {ex.Message}");
            }
        }

        private async Task<ResponseDTO<ImportSupplementReportDto>> HandleTransferSupplementAsync(SupplementImportRequestDto request)
        {
            try
            {
                // 1. Validate
                if (request.OriginalImportId <= 0)
                    throw new ArgumentException("OriginalImportId không hợp lệ.");
                if (request.ImportDetails == null || !request.ImportDetails.Any())
                    throw new ArgumentException("Không có thông tin import detail.");

                // 2. Load old import
                var oldImport = await _impRepos.GetByIdAsyncWithDetails(request.OriginalImportId);
                if (oldImport == null)
                    throw new ArgumentException("Đơn gốc không tồn tại.");

                // 3. Compute missing quantities
                var transferDetailsDto = oldImport.ImportDetails
                    .Select(od => new
                    {
                        VariantId = od.ProductVariantId,
                        MissingStores = od.ImportStoreDetails
                            .Where(s => (s.ActualReceivedQuantity ?? 0) < s.AllocatedQuantity)
                            .ToList()
                    })
                    .Where(x => x.MissingStores.Any())
                    .Select(x => new TransImportDetailDto
                    {
                        ProductVariantId = x.VariantId,
                        Quantity = x.MissingStores.Sum(s => s.AllocatedQuantity - (s.ActualReceivedQuantity ?? 0))
                    })
                    .ToList();

                if (!transferDetailsDto.Any())
                    return new ResponseDTO<ImportSupplementReportDto>(null, false,
                        "Không có sản phẩm nào cần chuyển bổ sung.");

                // 4. Build import entity
                var importEntity = new Import
                {
                    CreatedBy = oldImport.CreatedBy,
                    CreatedDate = DateTime.Now,
                    ApprovedDate = DateTime.Now,
                    Status = "Pending",
                    ImportType = "Transfer",
                    ReferenceNumber = oldImport.ReferenceNumber,
                    IsUrgent = oldImport.IsUrgent,
                    OriginalImportId = oldImport.ImportId,
                    ImportDetails = transferDetailsDto
                        .Select(d => new ImportDetail
                        {
                            ProductVariantId = d.ProductVariantId,
                            Quantity = d.Quantity,
                            CostPrice = 0m,
                        })
                        .ToList()
                };
                importEntity.TotalCost = 0m;

                // 5. Save import + audit
                await _impRepos.AddAsync(importEntity);
                await _impRepos.SaveChangesAsync();
                await CreateAuditLogAsync("Import", importEntity.ImportId.ToString(), importEntity.CreatedBy,
                    "Tạo supplement chuyển kho");

                // 6. Auto-approve
                var transDto = new TransImportDto
                {
                    CreatedBy = oldImport.CreatedBy,
                    WarehouseId = 0, // dummy, will use per-store below
                    ImportDetails = transferDetailsDto,
                    IsUrgent = (bool)oldImport.IsUrgent
                };
                var capableMap = await EvaluateAutoApprovalAsync(importEntity, transDto);

                // 7. Build and save store details per missing store
                var storeDetails = new List<ImportStoreDetail>();
                foreach (var newDet in importEntity.ImportDetails)
                {
                    var oldDet = oldImport.ImportDetails
                        .First(od => od.ProductVariantId == newDet.ProductVariantId);
                    var missingStores = oldDet.ImportStoreDetails
                        .Where(s => (s.ActualReceivedQuantity ?? 0) < s.AllocatedQuantity)
                        .ToList();

                    foreach (var ms in missingStores)
                    {
                        var missingQty = ms.AllocatedQuantity - (ms.ActualReceivedQuantity ?? 0);
                        if (missingQty <= 0) continue;

                        // Create detail using ms.WarehouseId
                        var status = capableMap[newDet.ProductVariantId] == "Tự động phê duyệt"
                            ? "Processing"
                            : "Rejected";

                        storeDetails.Add(new ImportStoreDetail
                        {
                            ImportDetailId = newDet.ImportDetailId,
                            WarehouseId = ms.WarehouseId,
                            ActualReceivedQuantity = null,
                            AllocatedQuantity = missingQty,
                            Status = status,
                            Comments = status == "Processing"
                                ? "Tự động tạo supplement"
                                : $"Rejected: {capableMap[newDet.ProductVariantId]}",
                            StaffDetailId = ms.StaffDetailId,
                            HandleBy = ms.HandleBy
                        });
                    }
                }
                await _importStoreRepo.AddRangeAsync(storeDetails);
                await _impRepos.SaveChangesAsync();
                foreach (var sd in storeDetails)
                {
                    await CreateAuditLogAsync("ImportStoreDetail", sd.ImportStoreId.ToString(), importEntity.CreatedBy,
                        sd.Status == "Processing"
                            ? "Detail supplement chuyển kho Processing"
                            : sd.Comments);
                }

                // 8. Update import status
                if (capableMap.Values.All(v => v == "Tự động phê duyệt"))
                    importEntity.Status = "Approved";
                else if (capableMap.Values.All(v => v.StartsWith("Không đủ")))
                    importEntity.Status = "Rejected";
                else
                    importEntity.Status = "Partially Approved";

                await _impRepos.UpdateAsync(importEntity);
                await _impRepos.SaveChangesAsync();



                // 9. If Rejected
                if (importEntity.Status == "Rejected")
                {
                    var dtoRejected = new ImportSupplementReportDto
                    {
                        ImportData = _mapper.Map<ImportDto>(importEntity),
                        ReportFileBase64 = null
                    };
                    return new ResponseDTO<ImportSupplementReportDto>(dtoRejected, true,
                        "Supplement chuyển kho bị từ chối toàn bộ.");
                }

                var oldStoreDetails = oldImport.ImportDetails
           .SelectMany(od => od.ImportStoreDetails)
           .ToList();

                foreach (var oldSd in oldStoreDetails)
                {
                    oldSd.Status = "Handled";
                }
                await _importStoreRepo.UpdateRangeAsync(oldStoreDetails);
                await _impRepos.SaveChangesAsync();

                // 10.2 Gán status cho đơn supplement
                importEntity.Status = "Supplement Created";
                await _impRepos.UpdateAsync(importEntity);
                await _impRepos.SaveChangesAsync();

                // 10. Dispatch -> Transfer -> DispatchDetail -> Export
                var dispatch = await CreateDispatchAsync(importEntity);
                var transfer = await CreateTransferAsync(importEntity, dispatch);
                var transferDtls = await CreateTransferDetailsAsync(transfer, storeDetails);
                var dispatchDtls = await CreateDispatchDetailsAsync(dispatch, transferDtls);
                var destinationWarehouseId = storeDetails.First().WarehouseId;

                var exportDtls = await CreateExportDetailsAsync(
                    dispatchDtls,
                    destinationWarehouseId: (int)destinationWarehouseId,
                    isUrgent: (bool)importEntity.IsUrgent
                );
                // 11. Generate report
                var freshImport = await _impRepos.GetByIdAsync(importEntity.ImportId);
                var reportBytes = _reportService.GenerateImportSupplementSlip(freshImport, oldImport);
                var reportBase64 = Convert.ToBase64String(reportBytes);

                return new ResponseDTO<ImportSupplementReportDto>(new ImportSupplementReportDto
                {
                    ImportData = _mapper.Map<ImportDto>(freshImport),
                    ReportFileBase64 = reportBase64
                }, true, "Tạo supplement chuyển kho thành công!");
            }
            catch (Exception ex)
            {
                return new ResponseDTO<ImportSupplementReportDto>(null, false,
                    $"Quá trình tạo supplement chuyển kho thất bại: {ex.Message}");
            }
        }

        // --- Helper methods ---

        private List<TransImportDetailDto> ComputeMissingQuantities(Import oldImport)
        {
            var list = new List<TransImportDetailDto>();
            foreach (var oldDetail in oldImport.ImportDetails)
            {
                var missing = oldDetail.ImportStoreDetails?
                    .Where(s => (s.ActualReceivedQuantity ?? 0) < s.AllocatedQuantity)
                    .Sum(s => s.AllocatedQuantity - (s.ActualReceivedQuantity ?? 0)) ?? 0;
                if (missing > 0)
                    list.Add(new TransImportDetailDto
                    {
                        ProductVariantId = oldDetail.ProductVariantId,
                        Quantity = missing
                    });
            }
            return list;
        }

        private async Task<Dictionary<int, string>> EvaluateAutoApprovalAsync(
            Import importEntity, TransImportDto dto)
        {
            var result = new Dictionary<int, string>();
            foreach (var det in dto.ImportDetails)
            {
                var stores = await GetStoresByAvailableStockAsync(det.ProductVariantId, dto.WarehouseId, dto.IsUrgent);
                var totalAvail = stores.Sum(s => s.available);
                result[det.ProductVariantId] = totalAvail >= det.Quantity
                    ? "Tự động phê duyệt"
                    : $"Không đủ sản phẩm ({totalAvail} có sẵn)";
            }
            return result;
        }

        private void UpdateImportStatus(Import importEntity, Dictionary<int, string> capableMap)
        {
            if (capableMap.Values.All(v => v == "Tự động phê duyệt"))
                importEntity.Status = "Approved";
            else if (capableMap.Values.All(v => v.StartsWith("Không đủ sản phẩm")))
                importEntity.Status = "Rejected";
            else
                importEntity.Status = "Partially Approved";
        }

        private async Task<List<ImportStoreDetail>> BuildAndSaveStoreDetailsAsync(
            Import importEntity,
            Import oldImport,
            Dictionary<int, string> capableMap)
        {
            var list = new List<ImportStoreDetail>();
            foreach (var newDet in importEntity.ImportDetails)
            {
                var oldDet = oldImport.ImportDetails.First(od => od.ProductVariantId == newDet.ProductVariantId);
                var missingStores = oldDet.ImportStoreDetails
                    .Where(s => (s.ActualReceivedQuantity ?? 0) < s.AllocatedQuantity)
                    .ToList();

                foreach (var ms in missingStores)
                {
                    var missQty = ms.AllocatedQuantity - (ms.ActualReceivedQuantity ?? 0);
                    if (missQty <= 0) continue;

                    var wh = await _warehouseRepo.GetByIdAsync((int)ms.WarehouseId);
                    var checkers = await _wsRepos.GetByWarehouseAndRoleAsync((int)ms.WarehouseId, "Checker");
                    var unused = GetInitialUnusedCheckers(wh.UnusedCheckerIds, checkers);

                    var staff = unused[_rng.Next(unused.Count)];

                    var comment = capableMap[newDet.ProductVariantId];
                    var status = comment == "Tự động phê duyệt" ? "Processing" : "Rejected";

                    list.Add(new ImportStoreDetail
                    {
                        ImportDetailId = newDet.ImportDetailId,
                        WarehouseId = ms.WarehouseId,
                        AllocatedQuantity = missQty,
                        Status = status,
                        Comments = status == "Processing" ? "Tự động tạo supplement" : $"Rejected: {comment}",
                        StaffDetailId = staff,
                        HandleBy = ms.HandleBy
                    });
                }
            }

            await _importStoreRepo.AddRangeAsync(list);
            await _impRepos.SaveChangesAsync();

            foreach (var sd in list)
            {
                await CreateAuditLogAsync("ImportStoreDetail", sd.ImportStoreId.ToString(), importEntity.CreatedBy,
                    sd.Status == "Processing" ? "Detail supplement chuyển kho Processing" : sd.Comments);
            }
            return list;
        }

        private List<int> GetInitialUnusedCheckers(string storedUnused, IEnumerable<WarehouseStaff> all)
        {
            var unused = string.IsNullOrWhiteSpace(storedUnused)
                ? all.Select(w => w.StaffDetailId).ToList()
                : storedUnused.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

            if (!unused.Any())
                unused = all.Select(w => w.StaffDetailId).ToList();

            return unused;
        }

        private async Task<Dispatch> CreateDispatchAsync(Import importEntity)
        {
            var dispatch = new Dispatch
            {
                CreatedBy = importEntity.CreatedBy,
                CreatedDate = DateTime.Now,
                Status = "Approved",
                ReferenceNumber = $"DP-{DateTime.Now:yyyyMMddHHmmss}",
                Remarks = "Xuất hàng tự động cho supplement",
                OriginalId = importEntity.ImportId
            };
            await _dispatchRepos.AddAsync(dispatch);
            await _dispatchRepos.SaveChangesAsync();
            await CreateAuditLogAsync("Dispatch", dispatch.DispatchId.ToString(), dispatch.CreatedBy,
                "Tạo Dispatch tự động sau phê duyệt supplement");
            return dispatch;
        }

        private async Task<Transfer> CreateTransferAsync(Import importEntity, Dispatch dispatch)
        {
            var transfer = new Transfer
            {
                ImportId = importEntity.ImportId,
                DispatchId = dispatch.DispatchId,
                CreatedBy = importEntity.CreatedBy,
                CreatedDate = DateTime.Now,
                Status = "Approved",
                Remarks = "Tự động tạo cho supplement chuyển kho"
            };
            await _transferRepo.AddAsync(transfer);
            await _impRepos.SaveChangesAsync();
            await CreateAuditLogAsync("Transfer", transfer.TransferOrderId.ToString(), transfer.CreatedBy,
                "Tạo Transfer tự động");
            return transfer;
        }

        private async Task<List<TransferDetail>> CreateTransferDetailsAsync(
            Transfer transfer,
            List<ImportStoreDetail> storeDetails)
        {
            var toTrans = storeDetails.Where(sd => sd.Status == "Processing").ToList();
            var list = toTrans.Select(sd => new TransferDetail
            {
                TransferOrderId = transfer.TransferOrderId,
                VariantId = sd.ImportDetail.ProductVariantId,
                Quantity = sd.AllocatedQuantity
            }).ToList();
            await _transferDetailRepo.AddRangeAndSaveAsync(list);
            foreach (var td in list)
                await CreateAuditLogAsync("TransferDetail", td.TransferOrderDetailId.ToString(), transfer.CreatedBy,
                    $"Tạo TransferDetail variant {td.VariantId}");
            return list;
        }

        private async Task<List<DispatchDetail>> CreateDispatchDetailsAsync(
            Dispatch dispatch,
            List<TransferDetail> transferDetails)
        {
            var list = transferDetails.Select(td => new DispatchDetail
            {
                DispatchId = dispatch.DispatchId,
                VariantId = td.VariantId,
                Quantity = td.Quantity
            }).ToList();
            await _dispatchDetailRepo.AddRangeAndSaveAsync(list);
            foreach (var dd in list)
                await CreateAuditLogAsync("DispatchDetail", dd.DispatchDetailId.ToString(), dispatch.CreatedBy,
                    $"Tạo DispatchDetail variant {dd.VariantId}");
            return list;
        }

        private async Task<List<StoreExportStoreDetail>> CreateExportDetailsAsync(
      List<DispatchDetail> dispatchDetails,
      int destinationWarehouseId,
      bool isUrgent)
        {
            var stores = await GetStoresByAvailableStockAsync(
    dispatchDetails.First().VariantId,
    excludeWarehouseId: destinationWarehouseId,
    isUrgent: isUrgent
);
            // Chọn kho và build export giống mẫu
            var allWh = await _warehouseRepo.GetAllAsync();
            var chosen = stores.FirstOrDefault();
            var whEntity = allWh.Single(w => w.WarehouseId == chosen.warehouseId);
            var checkers = await _wsRepos.GetByWarehouseAndRoleAsyncNormal(whEntity.WarehouseId, "Checker");
            var unused = GetInitialUnusedCheckers(whEntity.UnusedCheckerIds, checkers);
            var list = BuildExportStoreDetails(
                dispatchDetails,
                warehouseId: whEntity.WarehouseId,
                shopManagerId: (int)whEntity.ShopManagerId,
                allCheckers: checkers,
                unused: unused,
                destinationId: destinationWarehouseId
            );
            await _storeExportRepo.AddRangeAndSaveAsync(list);
            foreach (var ed in list)
                await CreateAuditLogAsync("StoreExportStoreDetail", ed.DispatchStoreDetailId.ToString(), dispatchDetails.First().Dispatch.CreatedBy,
                    $"Tạo export detail cho DispatchDetail {ed.DispatchDetailId}");
            return list;
        }

        private async Task<List<(int warehouseId, int available)>> GetStoresByAvailableStockAsync(
            int variantId,
            int excludeWarehouseId,
            bool isUrgent)
        {
            var allWh = (await _warehouseRepo.GetAllAsync())
                .Where(w => w.WarehouseId != excludeWarehouseId)
                .ToList();
            var list = new List<(int, int)>();
            foreach (var wh in allWh)
            {
                var stock = await _stockRepo.GetByWarehouseAndVariantAsync(wh.WarehouseId, variantId);
                var qty = stock?.StockQuantity ?? 0;
                var safety = isUrgent ? (wh.UrgentSafetyStock ?? wh.SafetyStock) : wh.SafetyStock;
                var pending = await _dispatchRepos.GetApprovedOutboundQuantityAsync(wh.WarehouseId, variantId);
                list.Add((wh.WarehouseId, qty - (int)safety - (int)pending));
            }
            return list;
        }

        private List<StoreExportStoreDetail> BuildExportStoreDetails(
     List<DispatchDetail> dispatchDetails,
     int warehouseId,
     int shopManagerId,
     List<WarehouseStaff> allCheckers,
     List<int> unused,
     int destinationId)
        {
            var result = new List<StoreExportStoreDetail>();

            for (int i = 0; i < dispatchDetails.Count; i++)
            {
                var dd = dispatchDetails[i];

                // Chọn checkerId an toàn: dùng unused[i] nếu có, ngược lại fallback checker đầu
                int checkerId = (unused != null && unused.Count > i)
                    ? unused[i]
                    : allCheckers[0].StaffDetailId;

                // Lấy entity Checker, fallback nếu không tìm thấy
                var checkerEntity = allCheckers
                    .SingleOrDefault(c => c.StaffDetailId == checkerId)
                    ?? allCheckers[0];

                result.Add(new StoreExportStoreDetail
                {
                    DispatchDetailId = dd.DispatchDetailId,
                    WarehouseId = warehouseId,
                    HandleBy = shopManagerId,
                    StaffDetailId = checkerEntity.StaffDetailId,
                    AllocatedQuantity = dd.Quantity,        // hoặc business logic khác
                    ActualQuantity = 0,   // hoặc 0 nếu chưa nhận
                    Status = "Processing",
                    DestinationId = destinationId        // hoặc trường phù hợp
                });
            }

            return result;
        }

        private async Task CreateAuditLogAsync(string table, string recordId, int changedBy, string comment)
        {
            var log = new AuditLog
            {
                TableName = table,
                RecordId = recordId,
                Operation = "CREATE",
                ChangeDate = DateTime.Now,
                ChangedBy = changedBy,
                ChangeData = string.Empty,
                Comment = comment
            };
            await _auditLogRepos.AddAsync(log);
            await _auditLogRepos.SaveChangesAsync();
        }
    }
}



