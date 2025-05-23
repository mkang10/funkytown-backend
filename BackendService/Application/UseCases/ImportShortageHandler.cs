using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ImportShortageHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IAuditLogRepository _auditLogRepos;
        private readonly IWareHousesStockRepository _wareHouseStockRepos;
        private readonly IStaffDetailRepository _staffDetailRepository;

        public ImportShortageHandler(IStaffDetailRepository staffDetailRepository, IWareHousesStockRepository wareHouseStockRepos, IImportRepos importRepos, IAuditLogRepository auditLogRepos)
        {
            _importRepos = importRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
            _staffDetailRepository = staffDetailRepository;
        }

        public async Task ImportIncompletedAsync(int importId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {

            var accountId = await _staffDetailRepository.GetAccountIdByStaffIdAsync(staffId);

            var import = await _importRepos.GetByIdAssignAsync(importId);
            if (import == null)
            {
                throw new Exception("Import không tồn tại");
            }
            var transfer = await _importRepos.GetTransferByImportIdAsync(import.ImportId);
            if (transfer != null &&
                !string.Equals(transfer.Dispatch?.Status?.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
            {
                // Không cho phép đánh dấu import Done nếu dispatch chưa Done
                throw new InvalidOperationException(
                    "Không thể hoàn thành nhập hàng khi đơn xuất hàng chưa có trạng thái Done"
                );
            }
            var currentStatus = import.Status.Trim();
            // Chỉ cho phép xử lý các Import có trạng thái Processing hoặc Partial Success
            if (!string.Equals(currentStatus, "Approved", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentStatus, "Shortage", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentStatus, "Processing", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentStatus, "Supplement Created", StringComparison.OrdinalIgnoreCase)&&
                !string.Equals(currentStatus, "Done", StringComparison.OrdinalIgnoreCase))

            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Approved , Partial Success, Shortage  và Supplement Created");
            }

            // Danh sách các store detail được cập nhật để dùng cho việc cập nhật tồn kho sau này
            var updatedStoreDetails = new List<(ImportStoreDetail storeDetail, int productVariantId)>();

            // Duyệt qua từng ImportDetail và ImportStoreDetail
            foreach (var importDetail in import.ImportDetails)
            {

                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    // Tìm thông tin xác nhận tương ứng
                    var confirmation = confirmations.FirstOrDefault(c => c.StoreDetailId == storeDetail.ImportStoreId);
                    if (confirmation == null)
                    {
                        continue;
                    }

                    // Kiểm tra số lượng thực nhận không vượt quá số lượng được phân bổ
                    if (confirmation.ActualReceivedQuantity > storeDetail.AllocatedQuantity)
                    {
                        throw new InvalidOperationException("ActualReceivedQuantity không được lớn hơn AllocatedQuantity, vui lòng nhập lại");
                    }

                    // Cập nhật ImportStoreDetail theo thông tin confirmation (chỉ những detail này mới bị ảnh hưởng)
                    storeDetail.Status = "Shortage";
                    storeDetail.Comments = string.IsNullOrEmpty(confirmation.Comment)
                                             ? "Hàng không đủ"
                                             : confirmation.Comment;
                    storeDetail.ActualReceivedQuantity = confirmation.ActualReceivedQuantity;

                    // Tạo AuditLog cho từng ImportStoreDetail được cập nhật
                    var auditLogDetail = new AuditLog
                    {
                        TableName = "ImportStoreDetail",
                        RecordId = storeDetail.ImportStoreId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = accountId,
                        ChangeData = $"Trạng thái được cập nhật thành thiếu hàng và số lượng thực tế được cập nhật : {storeDetail.ActualReceivedQuantity}",
                        Comment = storeDetail.Comments
                    };
                    _auditLogRepos.Add(auditLogDetail);

                    // Lưu lại store detail và variantId để cập nhật tồn kho sau
                    updatedStoreDetails.Add((storeDetail, importDetail.ProductVariantId));
                }
            }

            // Lưu các thay đổi cho ImportStoreDetail và audit log
            await _importRepos.SaveChangesAsync();

            // Nếu có bất kỳ ImportStoreDetail nào có trạng thái Shortage, cập nhật Import status thành Partial Success
            if (import.ImportDetails.Any(id => id.ImportStoreDetails.Any(sd => string.Equals(sd.Status, "Shortage", StringComparison.OrdinalIgnoreCase))))
            {
                import.Status = "Shortage";
                import.CompletedDate = DateTime.Now;

                var auditLogImport = new AuditLog
                {
                    TableName = "Import",
                    RecordId = import.ImportId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = "Status updated to Partial Success",
                    Comment = "Trạng thái đơn nhập hàng được cập nhật thành Shortage"
                };
                _auditLogRepos.Add(auditLogImport);

                await _importRepos.SaveChangesAsync();
            }

            // Cập nhật tồn kho cho từng ImportStoreDetail đã được cập nhật bằng hàm UpdateWarehouseStockForSingleDetailAsync
            foreach (var item in updatedStoreDetails)
            {
                await _wareHouseStockRepos.UpdateWarehouseStockForSingleDetailAsync(item.storeDetail, item.productVariantId, staffId);

                if (string.Equals(import.ImportType?.Trim(), "Purchase", StringComparison.OrdinalIgnoreCase))
                {
                    await UpdateVariantPricesAsync(import, staffId);
                }
            }
            if (transfer != null)
            {
                // Nếu transfer chưa ở trạng thái Done, cứ đặt về Processing
                if (!string.Equals(transfer.Status, "Done", StringComparison.OrdinalIgnoreCase))
                {
                    transfer.Status = "Processing";
                    // Tạo audit log cho transfer
                    _auditLogRepos.Add(new AuditLog
                    {
                        TableName = "Transfer",
                        RecordId = transfer.TransferOrderId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = accountId,
                        ChangeData = $"Status set to Processing because child import {import.ImportId} updated",
                        Comment = "Đơn chyển đang được xử lí !"
                    });
                }
            }


            // Lưu lại các bản ghi audit được thêm trong quá trình cập nhật tồn kho (nếu có)
            await _auditLogRepos.SaveChangesAsync();


        }
        private async Task UpdateVariantPricesAsync(Import import, int staffId)
        {
            // Lấy danh sách variant đã có trong import này
            var variantIds = import.ImportDetails
                                   .Select(d => d.ProductVariantId)
                                   .Distinct();

            foreach (var variantId in variantIds)
            {
                var accountId = await _staffDetailRepository.GetAccountIdByStaffIdAsync(staffId);
                if (accountId == null)
                    throw new KeyNotFoundException($"Không tìm thấy Account cho StaffId={staffId}");
                // 1) Lấy tất cả ImportDetail cho variant này
                var allDetails = await _importRepos.QueryImportDetails()
                    .Include(d => d.Import)
                    .Where(d => d.ProductVariantId == variantId)
                    .ToListAsync();

                // 2) Loại trừ detail từ những Import gắn với Transfer
                var validDetails = new List<ImportDetail>();
                foreach (var det in allDetails)
                {
                    var isFromTransfer = await _importRepos.HasTransferForImportAsync(det.ImportId);
                    if (!isFromTransfer)
                        validDetails.Add(det);
                }

                // 3) Tính tổng số lượng
                var totalQty = validDetails.Sum(d => d.Quantity);
                if (totalQty == 0)
                    continue;   // không có data để cập nhật

                // 4) Tính tổng giá vốn
                var totalCost = validDetails.Sum(d => (d.CostPrice ?? 0m) * d.Quantity);
                var avgCost = totalCost / totalQty;

                // 5) Cộng thêm 30% lợi nhuận
                var profitRate = 0.30m;
                var avgCostWithProfit = avgCost * (1 + profitRate);

                // 6) Làm tròn đến hàng đơn vị (0 chữ số sau dấu thập phân)
                //    MidpointRounding.AwayFromZero để .5 trở lên sẽ làm tròn lên
                var finalPrice = Math.Round(avgCostWithProfit, 0, MidpointRounding.AwayFromZero);

                // 7) Cập nhật vào bảng ProductVariant
                var variant = await _importRepos.GetProductVariantByIdAsync(variantId);
                variant.Price = finalPrice;

                // 8) Tạo AuditLog cho thay đổi giá
                var log = new AuditLog
                {
                    TableName = "ProductVariant",
                    RecordId = variantId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = $"{{ \"OldPrice\": ..., \"NewPrice\": {avgCost} }}",
                    Comment = "Cập nhật giá trung bình sau khi hoàn thành nhập"
                };
                _auditLogRepos.Add(log);
            }
        }

    }
}