// File: ImportDoneHandler.cs
using Azure;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ImportDoneHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IAuditLogRepository _auditLogRepos;
        private readonly IWareHousesStockRepository _wareHouseStockRepos;
        private readonly IStaffDetailRepository _staffDetail;

        public ImportDoneHandler(
            IStaffDetailRepository staffDetail,
            IWareHousesStockRepository wareHouseStockRepos,
            IImportRepos importRepos,
            IAuditLogRepository auditLogRepos)
        {
            _importRepos = importRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
            _staffDetail = staffDetail;
        }

        public async Task ProcessImportDoneAsync(int importId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {
            var import = await _importRepos.GetByIdAsync(importId);
            if (import == null)
                throw new Exception("Import không tồn tại");
            var transfer = await _importRepos.GetTransferByImportIdAsync(import.ImportId);
            if (transfer != null &&
                !string.Equals(transfer.Dispatch?.Status?.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
            {
                // Không cho phép đánh dấu import Done nếu dispatch chưa Done
                throw new InvalidOperationException(
                    "Không thể hoàn thành nhập hàng khi đơn xuất hàng chưa có trạng thái Done"
                );
            }
            ValidateImportStatus(import);
            await UpdateImportStoreDetails(import, confirmations, staffId);
            await _importRepos.SaveChangesAsync();

            import = await _importRepos.GetByIdAsync(importId);

            var confirmedIds = confirmations.Select(c => c.StoreDetailId).ToList();
            if (AllDetailsSuccess(import))
            {
                // Kiểm tra xem import có thuộc transfer và dispatch đã hoàn thành hay chưa
                

                MarkImportDone(import, staffId);
                await _importRepos.SaveChangesAsync();

                await UpdateOriginalImportFromSupplementsAsync(import, staffId);
                await _wareHouseStockRepos.UpdateWarehouseStockAsync(import, staffId, confirmedIds);
            }
            else
            {
                await UpdateWarehouseForSuccessDetailsAsync(import, staffId);
            }

            if (string.Equals(import.ImportType?.Trim(), "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                await UpdateVariantPricesAsync(import, staffId);
            }

            await _importRepos.SaveChangesAsync();
            await _auditLogRepos.SaveChangesAsync();

            await UpdateTransferStatusAsync(import, staffId);

            if (transfer != null)
            {
                await UpdateOriginalTransferFromChildrenAsync(transfer, staffId);
            }

        }

        #region Helper Methods

        private void ValidateImportStatus(Import import)
        {
            var allowed = new[] { "Approved", "Shortage", "Processing", "Supplement Created","Done" };
            var status = import.Status?.Trim();
            if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Approved, Processing, Shortage và Supplement Created");
        }

        private bool AllDetailsSuccess(Import import)
        {
            var stores = import.ImportDetails.SelectMany(d => d.ImportStoreDetails);
            return stores.Any() && stores.All(sd => string.Equals(sd.Status?.Trim(), "Success", StringComparison.OrdinalIgnoreCase));
        }

        private void MarkImportDone(Import import, int staffId)
        {
            import.Status = "Done";
            import.CompletedDate = DateTime.Now;
            var audit = new AuditLog
            {
                TableName = "Import",
                RecordId = import.ImportId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.Now,
                ChangedBy = _staffDetail.GetAccountIdByStaffIdAsync(staffId).Result,
                ChangeData = JsonConvert.SerializeObject(import, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                Comment = "Cập nhật đơn nhập hàng thành Done"
            };
            _auditLogRepos.Add(audit);
        }

        private async Task UpdateImportStoreDetails(Import import, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);

            var stores = import.ImportDetails.SelectMany(d => d.ImportStoreDetails).ToList();
            foreach (var dto in confirmations)
            {
                var sd = stores.FirstOrDefault(s => s.ImportStoreId == dto.StoreDetailId)
                         ?? throw new Exception($"Không tìm thấy ImportStoreDetail với ID {dto.StoreDetailId}");

                sd.ActualReceivedQuantity = sd.AllocatedQuantity;
                sd.Status = "Success";

                _auditLogRepos.Add(new AuditLog
                {
                    TableName = "ImportStoreDetail",
                    RecordId = sd.ImportStoreId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = $"Status: Success, ActualReceivedQuantity: {sd.ActualReceivedQuantity}",
                    Comment = "Đơn hàng đã được nhập vào kho thành công !"
                });

                await PropagateSuccessInMemoryAsync(import, sd.WarehouseId.Value, sd.StaffDetailId.Value);
            }
        }

        private async Task UpdateWarehouseForSuccessDetailsAsync(Import import, int staffId)
        {
            var stores = await _importRepos.QueryImportStoreDetailsByImportId(import.ImportId).ToListAsync();
            var successes = stores.Where(sd => string.Equals(sd.Status?.Trim(), "Success", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!stores.Any()) return;

            foreach (var sd in successes)
            {
                await _wareHouseStockRepos.UpdateWarehouseStockForSingleDetailAsync(sd, sd.ImportDetail.ProductVariantId, staffId);
            }

            import.Status = stores.Any(sd => sd.Status.Trim().Equals("Shortage", StringComparison.OrdinalIgnoreCase))
                ? "Shortage"
                : (successes.Count == stores.Count ? "Done" : "Processing");
        }

        private async Task UpdateOriginalImportFromSupplementsAsync(Import import, int staffId)
        {
            if (!import.OriginalImportId.HasValue) return;

            var supplements = await _importRepos.GetAllByOriginalImportIdAsync(import.OriginalImportId.Value);
            if (supplements.All(s => string.Equals(s.Status?.Trim(), "Done", StringComparison.OrdinalIgnoreCase)))
            {
                var original = await _importRepos.GetByIdAsync(import.OriginalImportId.Value);
                PropagateSupplementsInMemory(original, supplements);
                MarkImportDone(original, staffId);
            }
        }



        private async Task UpdateVariantPricesAsync(Import import, int staffId)
        {
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
            var variantIds = import.ImportDetails.Select(d => d.ProductVariantId).Distinct();
            foreach (var vid in variantIds)
            {
                var details = await _importRepos.QueryImportDetails()
                    .Where(d => d.ProductVariantId == vid)
                    .ToListAsync();
                var valid = new List<ImportDetail>();
                foreach (var d in details)
                {
                    if (!await _importRepos.HasTransferForImportAsync(d.ImportId)) valid.Add(d);
                }
                var totalQty = valid.Sum(d => d.Quantity);
                if (totalQty == 0) continue;

                var avgCost = valid.Sum(d => (d.CostPrice ?? 0) * d.Quantity) / totalQty;
                var finalPrice = Math.Round(avgCost * 1.3m, 0, MidpointRounding.AwayFromZero);

                var variant = await _importRepos.GetProductVariantByIdAsync(vid);
                variant.Price = finalPrice;

                _auditLogRepos.Add(new AuditLog
                {
                    TableName = "ProductVariant",
                    RecordId = vid.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = $"OldPrice->NewPrice: {avgCost}",
                    Comment = "Cập nhật giá trung bình sau khi hoàn thành nhập"
                });
            }
        }

        private async Task UpdateTransferStatusAsync(Import import, int staffId)
        {
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
            var transfer = await _importRepos.GetTransferByImportIdAsync(import.ImportId);
            if (transfer == null) return;

            if (import.Status == "Done" && transfer.Dispatch?.Status == "Done")
            {
                transfer.Status = "Done";
                _auditLogRepos.Add(new AuditLog
                {
                    TableName = "Transfer",
                    RecordId = transfer.TransferOrderId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = "Status updated to Done",
                    Comment = "Đơn chuyển hàng đã hoàn thành !"
                });
                await _importRepos.SaveChangesAsync();
                await _auditLogRepos.SaveChangesAsync();
            }
        }

        #endregion

        #region In-Memory Propagation Helpers

        private async Task PropagateSuccessInMemoryAsync(Import current, int warehouseId, int staffDetailId)
        {
            var visited = new HashSet<int>();

            while (current.OriginalImportId.HasValue)
            {
                if (!visited.Add(current.ImportId))
                {
                    // Đã duyệt Import này trước đó => vòng lặp => thoát
                    throw new InvalidOperationException("Phát hiện vòng lặp trong chuỗi OriginalImportId.");
                }

                var parent = await _importRepos.GetByIdAsync(current.OriginalImportId.Value)
                             ?? throw new Exception($"Không tìm thấy Import gốc với ID {current.OriginalImportId.Value}");

                var store = parent.ImportDetails
                    .SelectMany(d => d.ImportStoreDetails)
                    .FirstOrDefault(sd => sd.WarehouseId == warehouseId && sd.StaffDetailId == staffDetailId);

                if (store != null)
                {
                    store.ActualReceivedQuantity = store.AllocatedQuantity;
                    store.Status = "Success";
                }

                if (parent.ImportDetails.SelectMany(d => d.ImportStoreDetails)
                    .All(sd => string.Equals(sd.Status?.Trim(), "Success", StringComparison.OrdinalIgnoreCase)))
                {
                    parent.Status = "Done";
                }

                current = parent;
            }
        }

        private async Task UpdateOriginalTransferFromChildrenAsync(Domain.Entities.Transfer currentTransfer, int staffId)
        {
            if (!currentTransfer.OriginalTransferOrderId.HasValue)
                return;

            var originalId = currentTransfer.OriginalTransferOrderId.Value;

            // Lấy tất cả các transfer con cùng originalId
            var children = await _importRepos
                .QueryTransfers()                      // Giả sử bạn có method trả IQueryable<Transfer>
                .Where(t => t.OriginalTransferOrderId == originalId)
                .ToListAsync();

            // Nếu có ít nhất 1 con và tất cả đều Done
            if (children.Any() && children.All(t => t.Status == "Done"))
            {
                var parent = await _importRepos.GetTransferByIdAsync(originalId);
                if (parent == null) return;

                parent.Status = "Done";
                var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);

                _auditLogRepos.Add(new AuditLog
                {
                    TableName = "Transfer",
                    RecordId = parent.TransferOrderId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = $"Status -> Done",
                    Comment = "Tự động cập nhật phiếu chuyển gốc khi tất cả phiếu chuyển bổ sung đều Done"
                });

                // Lưu ngay để tránh race condition
                await _importRepos.SaveChangesAsync();
                await _auditLogRepos.SaveChangesAsync();

                // Đệ quy tiếp lên các level cao hơn (nếu có)
                await UpdateOriginalTransferFromChildrenAsync(parent, staffId);
            }
        }


        private void PropagateSupplementsInMemory(Import original, List<Import> supplements)
        {
            foreach (var detail in original.ImportDetails)
            {
                foreach (var store in detail.ImportStoreDetails)
                {
                    var matched = supplements
                        .SelectMany(s => s.ImportDetails)
                        .SelectMany(d => d.ImportStoreDetails)
                        .Where(s => s.WarehouseId == store.WarehouseId && s.StaffDetailId == store.StaffDetailId)
                        .ToList();

                    if (matched.Any(m => string.Equals(m.Status?.Trim(), "Success", StringComparison.OrdinalIgnoreCase)))
                    {
                        store.Status = "Success";
                        store.ActualReceivedQuantity = store.AllocatedQuantity;
                    }
                }
            }

            if (original.ImportDetails.SelectMany(d => d.ImportStoreDetails).All(s => s.Status == "Success"))
            {
                original.Status = "Done";
            }
        }


        #endregion
    }
}