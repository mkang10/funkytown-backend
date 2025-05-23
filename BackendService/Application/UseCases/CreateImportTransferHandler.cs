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
    public class CreateImportTransferHandler
    {
        private readonly Random _rng = new();

        private readonly IWarehouseRepository _warehouseRepo;
        private readonly IWareHousesStockRepository _stockRepo;
        private readonly IProductVarRepos _productVariantRepo;
        private readonly ITransferRepos _transferRepo;
        private readonly ITransferDetailRepository _transferDetailRepo;
        private readonly IImportRepos _importRepo;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IImportStoreRepos _importStoreRepo;
        private readonly IWarehouseStaffRepos _wsRepos;
        private readonly IDispatchDetailRepository _dispatchDetail;
        private readonly IStoreExportRepos _storeExportRepos;
        private readonly IStaffDetailRepository _staffRepos;

        public CreateImportTransferHandler(
            IWarehouseRepository warehouseRepo,
            IWareHousesStockRepository stockRepo,
            IProductVarRepos productVariantRepo,
            ITransferRepos transferRepo,
            ITransferDetailRepository transferDetailRepo,
            IImportRepos importRepo,
            IAuditLogRepository auditLogRepo,
            IDispatchRepos dispatchRepos,
            IImportStoreRepos importStoreRepo,
            IWarehouseStaffRepos wsRepos,
            IDispatchDetailRepository dispatchDetail,
            IStoreExportRepos storeExportRepos,
            IStaffDetailRepository staffRepos)
        {
            _warehouseRepo = warehouseRepo;
            _stockRepo = stockRepo;
            _productVariantRepo = productVariantRepo;
            _transferRepo = transferRepo;
            _transferDetailRepo = transferDetailRepo;
            _importRepo = importRepo;
            _auditLogRepo = auditLogRepo;
            _dispatchRepos = dispatchRepos;
            _importStoreRepo = importStoreRepo;
            _wsRepos = wsRepos;
            _dispatchDetail = dispatchDetail;
            _storeExportRepos = storeExportRepos;
            _staffRepos = staffRepos;
        }

        public async Task<ResponseDTO<ImportResponseDto>> CreateTransferImportAsync(TransImportDto dto)
        {
            try
            {
                var validation = ValidateDto(dto) ?? await ValidateStockLimitsAsync(dto);
                if (validation != null)
                    return validation;

                var importEntity = BuildImportEntity(dto);


                var capableMap = await EvaluateAutoApprovalAsync(importEntity, dto);

                await SaveImportAsync(importEntity);

                await CreateAuditLogAsync("Import", importEntity.ImportId.ToString(), importEntity.CreatedBy, "Tạo yêu cầu nhập hàng");


                var warehouse = await _warehouseRepo.GetByIdAsync(dto.WarehouseId);
                if (warehouse == null)
                    return new ResponseDTO<ImportResponseDto>(null, false, "Không tìm thấy warehouse.");

                var allWarehouses = await _warehouseRepo.GetAllAsync();

                var otherWarehouseIds = allWarehouses
                    .Where(w => w.WarehouseId != dto.WarehouseId)
                    .Select(w => w.WarehouseId)
                    .ToList();


                // 5. Allocate Checkers
                var allCheckers = await _wsRepos.GetByWarehouseAndRoleAsyncNormal(warehouse.WarehouseId, "Checker");
                var unused = GetInitialUnusedCheckers(warehouse.UnusedCheckerIds, allCheckers);

                var exportCheckers = new List<WarehouseStaff>();
                foreach (var wid in otherWarehouseIds)
                {
                    var checkersInOther = await _wsRepos.GetByWarehouseAndRoleAsyncNormal(wid, "Checker");
                    exportCheckers.AddRange(checkersInOther);
                }
                var exportUnused = GetInitialUnusedCheckers(warehouse.UnusedCheckerIds, exportCheckers);

                // 6. Create and save ImportStoreDetails
                var storeDetails = BuildStoreDetails(importEntity, dto.WarehouseId, (int)warehouse.ShopManagerId, allCheckers, unused, capableMap);
                await _importStoreRepo.AddRangeAsync(storeDetails);
                await _importRepo.SaveChangesAsync();


                // 7. Audit log for each StoreDetail
                foreach (var sd in storeDetails)
                {
                    string comment = sd.Status == "Processing"
                        ? $"Tạo mới detail cho import {importEntity.ImportId}"
                        : "Tạo detail với trạng thái Rejected do không đủ khả năng nhập";
                    await CreateAuditLogAsync("ImportStoreDetail", sd.ImportStoreId.ToString(), importEntity.CreatedBy, comment);
                }

                if (capableMap.Values.All(v => v == "Tự động phê duyệt"))
                    importEntity.Status = "Approved";
                else if (capableMap.Values.All(v => v == "Không đủ sản phẩm"))
                    importEntity.Status = "Rejected";
                else
                    importEntity.Status = "Partially Approved";


                await _importRepo.UpdateAsync(importEntity);
                await _importRepo.SaveChangesAsync();

                // 8. Update Warehouse unused list
                warehouse.UnusedCheckerIds = string.Join(",", unused);
                await _warehouseRepo.UpdateAsync(warehouse);
                //Nếu bị từ chối thì trả về luôn với message cụ thể
                // === BỔ SUNG: Nếu import bị REJECTED thì cập nhật các ImportStoreDetail thành "Rejected" ===
                if (string.Equals(importEntity.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var sd in storeDetails)
                    {
                        // Chuyển status
                        sd.Status = "Rejected";
                        await _importStoreRepo.UpdateAsync(sd);

                        // Audit log
                        await CreateAuditLogAsync(
                            table: "ImportStoreDetail",
                            recordId: sd.ImportStoreId.ToString(),
                            changedBy: importEntity.CreatedBy,
                            comment: "Cập nhật trạng thái từ Processing → Rejected do đơn nhập hàng bị từ chối"
                        );
                    }
                    // Lưu các thay đổi
                    await _importRepo.SaveChangesAsync();

                    // Trả về luôn (Status = true), client sẽ biết import đã xử lý xong nhưng status là Rejected
                    var rejectedDto = new ImportResponseDto { ImportId = importEntity.ImportId };
                    return new ResponseDTO<ImportResponseDto>(
                        rejectedDto,
                        true,
                        "Đơn nhập đã bị từ chối, các đơn con đã được chuyển sang trạng thái Rejected."
                    );
                }
                if (importEntity.Status == "Approved" || importEntity.Status == "Partially Approved")
                {
                    // Lọc chỉ những storeDetails có Status == "Processing"
                    var toDispatch = storeDetails.Where(sd => sd.Status == "Processing").ToList();
                    if (toDispatch.Any())
                    {
                        // 9.1 Tạo Dispatch
                        var dispatch = new Dispatch
                        {
                            CreatedBy = importEntity.CreatedBy,
                            CreatedDate = DateTime.Now,
                            Status = "Approved",
                            ReferenceNumber = $"DP-{DateTime.Now:yyyyMMddHHmmss}",
                            Remarks = "Đơn xuất hàng tự động cho chuyển hàng",
                            OriginalId = importEntity.ImportId
                        };
                        await _dispatchRepos.AddAsync(dispatch);
                        await _dispatchRepos.SaveChangesAsync();
                        await CreateAuditLogAsync("Dispatch", dispatch.DispatchId.ToString(), dispatch.CreatedBy,
                            "Tạo đơn xuất hàng tự động sau khi phê duyệt đơn nhập hàng thành công");

                        // 9.2 Tạo Transfer
                        var transfer = new Transfer
                        {
                            ImportId = importEntity.ImportId,
                            DispatchId = dispatch.DispatchId,
                            CreatedBy = importEntity.CreatedBy,
                            CreatedDate = DateTime.Now,
                            Status = "Approved",
                            Remarks = "Tự động được tạo khi đơn nhập hàng được phê duyệt"
                        };
                        await _transferRepo.AddAsync(transfer);
                        await _importRepo.SaveChangesAsync();
                        await CreateAuditLogAsync("Transfer", transfer.TransferOrderId.ToString(), transfer.CreatedBy,
                            "Tạo đơn chuyển hàng tự động liên kết với đơn xuất hàng");

                        // 9.3 Tạo TransferDetail chỉ từ toDispatch
                        var transferDetails = toDispatch
                            .Select(sd => new TransferDetail
                            {
                                TransferOrderId = transfer.TransferOrderId,
                                VariantId = sd.ImportDetail.ProductVariantId,  // hoặc ánh xạ tương ứng
                                Quantity = sd.AllocatedQuantity
                            })
                            .ToList();
                        await _transferDetailRepo.AddRangeAndSaveAsync(transferDetails);
                        foreach (var td in transferDetails)
                        {
                            await CreateAuditLogAsync("TransferDetail", td.TransferOrderDetailId.ToString(), transfer.CreatedBy,
                                $"Tạo đơn chuyển hàng chi tiết (variant {td.VariantId})");
                        }

                        // 9.4 Tạo DispatchDetail chỉ từ transferDetails
                        var dispatchDetails = transferDetails
                            .Select(td => new DispatchDetail
                            {
                                DispatchId = dispatch.DispatchId,
                                VariantId = td.VariantId,
                                Quantity = td.Quantity
                            })
                            .ToList();
                        await _dispatchDetail.AddRangeAndSaveAsync(dispatchDetails);
                        await _dispatchRepos.SaveChangesAsync();
                        foreach (var dd in dispatchDetails)
                        {
                            await CreateAuditLogAsync("DispatchDetail", dd.DispatchDetailId.ToString(), dispatch.CreatedBy,
                                $"Tạo đơn xuất hàng chi tiết (variant {dd.VariantId})");
                        }

                        // 9.5 Tạo StoreExportStoreDetail chỉ từ dispatchDetails
                        int totalQty = dispatchDetails.Sum(dd => dd.Quantity);
                        var stores = await GetStoresByAvailableStockAsync(
                            variantId: dispatchDetails[0].VariantId,
                            excludeWarehouseId: dto.WarehouseId,
                            isUrgent: dto.IsUrgent
                        );

                        var exportDetails = new List<StoreExportStoreDetail>();
                        var singleStore = stores.FirstOrDefault(s => s.available >= totalQty);
                        if (singleStore != default)
                        {
                            var whEntity = allWarehouses.Single(w => w.WarehouseId == singleStore.warehouseId);
                            var checkers = exportCheckers.Where(c => c.WarehouseId == whEntity.WarehouseId).ToList();
                            var unusedCheckers = GetInitialUnusedCheckers(whEntity.UnusedCheckerIds, checkers);
                            exportDetails = BuildExportStoreDetails(
                                dispatchDetails,
                                warehouseId: whEntity.WarehouseId,
                                shopManagerId: (int)whEntity.ShopManagerId,
                                allCheckers: checkers,
                                unused: unusedCheckers,
                                destinationId: dto.WarehouseId
                            );
                        }
                        else
                        {
                            int remaining = totalQty;
                            foreach (var s in stores)
                            {
                                if (remaining <= 0) break;
                                if (s.available <= 0) continue;

                                int take = Math.Min(s.available, remaining);
                                var partialDispatches = dispatchDetails
                                    .Select(dd => new DispatchDetail
                                    {
                                        DispatchDetailId = dd.DispatchDetailId,
                                        VariantId = dd.VariantId,
                                        Quantity = (int)Math.Ceiling((double)dd.Quantity * take / totalQty)
                                    })
                                    .ToList();

                                var diff = partialDispatches.Sum(pd => pd.Quantity) - take;
                                if (diff != 0)
                                    partialDispatches[0].Quantity -= diff;

                                var whEntity = allWarehouses.Single(w => w.WarehouseId == s.warehouseId);
                                var checkers = exportCheckers.Where(c => c.WarehouseId == whEntity.WarehouseId).ToList();
                                var unusedCheckers = GetInitialUnusedCheckers(whEntity.UnusedCheckerIds, checkers);
                                var partDetails = BuildExportStoreDetails(
                                    partialDispatches,
                                    warehouseId: whEntity.WarehouseId,
                                    shopManagerId: (int)whEntity.ShopManagerId,
                                    allCheckers: checkers,
                                    unused: unusedCheckers,
                                    destinationId: dto.WarehouseId
                                );
                                exportDetails.AddRange(partDetails);
                                remaining -= take;
                            }

                            if (remaining > 0)
                                throw new InvalidOperationException("Tổng hàng tồn của các kho vẫn không đủ để xuất.");
                        }

                        // 9.7 Lưu và audit log cho exportDetails
                        await _storeExportRepos.AddRangeAndSaveAsync(exportDetails);
                        foreach (var ed in exportDetails)
                        {
                            await CreateAuditLogAsync("StoreExportStoreDetail", ed.DispatchStoreDetailId.ToString(), dispatch.CreatedBy,
                                $"Tạo export detail cho DispatchDetail {ed.DispatchDetailId} từ kho {ed.WarehouseId}");
                        }
                    }
                }



                var resultDto = new ImportResponseDto
                {
                    ImportId = importEntity.ImportId,
                };


                return new ResponseDTO<ImportResponseDto>(
        new ImportResponseDto { ImportId = importEntity.ImportId },
        true,
        importEntity.Status == "Rejected"
            ? "Tất cả các detail đều bị từ chối."
            : importEntity.Status == "Approved"
                ? "Tất cả detail đều được duyệt."
                : "Một số detail bị từ chối, các detail còn lại đã được tiếp tục xử lý."
    );
            }
            catch (Exception ex)
            {
                return new ResponseDTO<ImportResponseDto>(null, false, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }



        public async Task<ResponseDTO<ImportResponseDto>> CreateTRansferImportFromExcelAsync(IFormFile file, int warehouseId, int createdBy)
        {
            if (file == null || file.Length == 0)
                return new ResponseDTO<ImportResponseDto>(null, false, "Vui lòng chọn file Excel.");

            var details = new List<TransImportDetailDto>();
            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var sheet = workbook.Worksheet(1);
                var firstRow = sheet.FirstRowUsed();
                if (firstRow == null)
                    return new ResponseDTO<ImportResponseDto>(null, false, "File Excel không có dữ liệu.");

                int row = firstRow.RowNumber() + 1;
                while (true)
                {
                    var skuCell = sheet.Cell(row, 1);
                    if (skuCell.IsEmpty()) break;

                    string sku = skuCell.GetString().Trim();
                    var variant = await _productVariantRepo.GetBySkuAsync(sku);
                    if (variant == null)
                        return new ResponseDTO<ImportResponseDto>(null, false, $"Dòng {row}: Không tìm thấy variant với SKU '{sku}'.");

                    int qty = sheet.Cell(row, 2).GetValue<int>();

                    details.Add(new TransImportDetailDto
                    {
                        ProductVariantId = variant.VariantId,
                        Quantity = qty,
                    });

                    row++;
                }
            }

            if (!details.Any())
                return new ResponseDTO<ImportResponseDto>(null, false, "File Excel không có dòng dữ liệu hợp lệ.");

            var dto = new TransImportDto
            {
                CreatedBy = createdBy,
                WarehouseId = warehouseId,
                ImportDetails = details,
                IsUrgent = false


            };

            return await CreateTransferImportAsync(dto);
        }
        private async Task<Dictionary<int, string>> EvaluateAutoApprovalAsync(Import importEntity, TransImportDto dto)
        {
            var detailComments = new Dictionary<int, string>();

            foreach (var detail in dto.ImportDetails)
            {
                var stores = await GetStoresByAvailableStockAsync(detail.ProductVariantId, dto.WarehouseId, dto.IsUrgent);
                var totalAvailable = stores.Where(s => s.available > 0).Sum(s => s.available);

                if (totalAvailable < detail.Quantity)
                    detailComments[detail.ProductVariantId] = "Không đủ sản phẩm";
                else
                    detailComments[detail.ProductVariantId] = "Tự động phê duyệt";
            }

            // không set importEntity.Status ở đây nữa
            return detailComments;
        }


        // Nếu tất cả chi tiết đều có thể nhập tại ít nhất 1 kho con

        private List<StoreExportStoreDetail> BuildExportStoreDetails(
    IEnumerable<DispatchDetail> dispatchDetails,
    int warehouseId,
    int shopManagerId,
    IEnumerable<WarehouseStaff> allCheckers,
    List<int> unused,
    int destinationId)
        {
            // Lấy danh sách staff còn unused; nếu hết thì reset
            if (!unused.Any())
                unused = allCheckers.Select(w => w.StaffDetailId).ToList();

            // Chọn 1 nhân viên random cho toàn bộ export của kho này
            int chosenStaff = unused[_rng.Next(unused.Count)];
            // Bỏ người đó khỏi danh sách unused
            unused.Remove(chosenStaff);

            var details = new List<StoreExportStoreDetail>();
            foreach (var dd in dispatchDetails)
            {
                details.Add(new StoreExportStoreDetail
                {
                    DispatchDetailId = dd.DispatchDetailId,
                    WarehouseId = warehouseId,
                    AllocatedQuantity = dd.Quantity,
                    Status = "Processing",
                    Comments = "Đơn xuất hàng được sinh tự động",
                    StaffDetailId = chosenStaff,
                    HandleBy = shopManagerId,
                    DestinationId = destinationId
                });
            }

            return details;
        }
        private async Task<List<(int warehouseId, int available)>> GetStoresByAvailableStockAsync(
    int variantId,
    int excludeWarehouseId,
    bool isUrgent)
        {
            // Lấy tất cả kho, loại bỏ kho đang import
            var allWarehouses = (await _warehouseRepo.GetAllAsync())
                .Where(w => w.WarehouseId != excludeWarehouseId)
                .ToList();

            // Ưu tiên kho con (IsOwnerWarehouse = false) trước, rồi kho tổng
            var storesOrdered = allWarehouses
     // những kho có IsOwnerWarehouse = true sẽ ở đầu
     .OrderByDescending(w => w.IsOwnerWarehouse)
     // nếu muốn tiếp tục sắp xếp theo tên hoặc một trường khác:
     .ToList();

            var result = new List<(int warehouseId, int available)>();

            foreach (var wh in storesOrdered)
            {
                var stock = await _stockRepo.GetByWarehouseAndVariantAsync(wh.WarehouseId, variantId);
                var qty = stock?.StockQuantity ?? 0;

                // Chọn mức safetyStock phù hợp
                var safetyThreshold = isUrgent
                    ? (wh.UrgentSafetyStock ?? wh.SafetyStock)
                    : wh.SafetyStock;

                var pendingOutbound = await _dispatchRepos.GetApprovedOutboundQuantityAsync(
                    wh.WarehouseId,
                    variantId
                );

                var available = qty - safetyThreshold - pendingOutbound;
                result.Add((wh.WarehouseId, (int)available));
            }

            return result;
        }

        private ResponseDTO<ImportResponseDto> ValidateDto(TransImportDto dto)
        {
            if (dto.ImportDetails == null || !dto.ImportDetails.Any())
                return new ResponseDTO<ImportResponseDto>(null, false, "Phải có ít nhất 1 sản phẩm.");
            return null;
        }

        // 2. Hàm ValidateStockLimitsAsync (chú ý dùng GetByIdAsync, không phải GetByIdsAsync)
        private async Task<ResponseDTO<ImportResponseDto>> ValidateStockLimitsAsync(TransImportDto dto)
        {
            foreach (var detail in dto.ImportDetails)
            {
                // Lấy stock hiện tại của variant
                var stock = await _stockRepo.GetByWarehouseAndVariantAsync(dto.WarehouseId, detail.ProductVariantId);
                var currentQty = stock?.StockQuantity ?? 0;

                // Lấy thông tin variant (dùng GetByIdAsync)
                var variant = await _productVariantRepo.GetByIdAsync(detail.ProductVariantId);
                if (variant == null)
                    return new ResponseDTO<ImportResponseDto>(null, false,
                        $"Không tìm thấy sản phẩm variant {detail.ProductVariantId}.");

                // Kiểm tra giới hạn tồn kho
                if (currentQty + detail.Quantity > variant.MaxStocks)
                {
                    var possibleQty = variant.MaxStocks - currentQty;
                    return new ResponseDTO<ImportResponseDto>(null, false,
                        $"Sản phẩm (ID {variant.VariantId}) hiện tại có {currentQty} trong kho, " +
                        $"yêu cầu thêm {detail.Quantity}, chỉ có thể nhập tối đa {possibleQty} nữa (tổng {variant.MaxStocks}).");
                }
            }

            return null;
        }




        private Import BuildImportEntity(TransImportDto dto)
        {
            var importEntity = new Import
            {
                CreatedBy = dto.CreatedBy,
                ApprovedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                Status = "Pending",
                ImportType = "Transfer",
                ReferenceNumber = GenerateReferenceNumber(),
                IsUrgent = (bool)dto.IsUrgent,
                ImportDetails = dto.ImportDetails.Select(d => new ImportDetail
                {
                    ProductVariantId = d.ProductVariantId,
                    Quantity = d.Quantity,
                    CostPrice = 0,
                }).ToList()
            };
            importEntity.TotalCost = 0;
            return importEntity;
        }

        private async Task SaveImportAsync(Import importEntity)
        {
            await _importRepo.AddAsync(importEntity);
            await _importRepo.SaveChangesAsync();
        }

        private string GenerateReferenceNumber()
            => $"IMP-TRS-{DateTime.Now:yyyyMMddHHmmss}";

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

        private List<ImportStoreDetail> BuildStoreDetails(
    Import importEntity,
    int warehouseId,
    int shopManagerId,
    IEnumerable<WarehouseStaff> allCheckers,
    List<int> unused,
    Dictionary<int, string> detailComments)
        {
            // Lấy danh sách staff còn unused; nếu hết thì reset
            if (!unused.Any())
                unused = allCheckers.Select(w => w.StaffDetailId).ToList();

            // Chọn 1 nhân viên random cho toàn bộ import trên warehouse này
            int chosenStaff = unused[_rng.Next(unused.Count)];
            // Bỏ người đó khỏi danh sách unused
            unused.Remove(chosenStaff);

            var details = new List<ImportStoreDetail>();
            foreach (var det in importEntity.ImportDetails)
            {
                var comment = detailComments[det.ProductVariantId];
                var status = comment.StartsWith("Không đủ sản phẩm") ? "Rejected" : "Processing";

                details.Add(new ImportStoreDetail
                {
                    ImportDetailId = det.ImportDetailId,
                    WarehouseId = warehouseId,
                    AllocatedQuantity = det.Quantity,
                    Status = status,
                    Comments = status == "Processing"
                                          ? "Tự động tạo bởi hệ thống"
                                          : $"Rejected: {comment}",
                    StaffDetailId = chosenStaff,
                    HandleBy = shopManagerId
                });
            }

            return details;
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
            await _auditLogRepo.AddAsync(log);
            await _auditLogRepo.SaveChangesAsync();
        }
    }
}
