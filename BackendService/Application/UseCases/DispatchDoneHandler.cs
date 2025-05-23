using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class DispatchDoneHandler
    {
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IImportRepos _importRepos;
        private readonly IStaffDetailRepository _staffRepos;
        private readonly IAuditLogRepository _auditLogRepos;
        private readonly IWareHousesStockRepository _wareHouseStockRepos;

        public DispatchDoneHandler(
            IStaffDetailRepository staffRepos,
            IImportRepos importRepos,
            IWareHousesStockRepository wareHouseStockRepos,
            IDispatchRepos dispatchRepos,
            IAuditLogRepository auditLogRepos)
        {
            _dispatchRepos = dispatchRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
            _importRepos = importRepos;
            _staffRepos = staffRepos;
        }

        public async Task ProcessDispatchDoneAsync(int dispatchId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {
            var dispatch = await _dispatchRepos.GetByIdAssignAsync(dispatchId)
                           ?? throw new Exception("Đơn xuất hàng không tồn tại");

            // Trim các giá trị status trước khi validate
            dispatch.Status = dispatch.Status?.Trim();
            ValidateDispatchStatus(dispatch);

            // 1. Cập nhật từng storeDetail
            await UpdateDispatchStoreDetails(dispatch, confirmations, staffId);
            await _dispatchRepos.SaveChangesAsync();
            await _dispatchRepos.ReloadAsync(dispatch);

            // 2. Lấy tất cả storeDetail & success list (trim Status trước khi so sánh)
            var allStores = dispatch.DispatchDetails
                                    .SelectMany(d => d.StoreExportStoreDetails)
                                    .ToList();
            var successStores = allStores
                .Where(sd => sd.Status?.Trim().Equals("Success", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
            var confirmedIds = confirmations.Select(c => c.StoreDetailId).ToList();

            // Chỉ mark Done khi tất cả store detail đều Success
            if (successStores.Count == allStores.Count)
            {
                MarkDispatchDone(dispatch, staffId);
                await _dispatchRepos.SaveChangesAsync();
                await _wareHouseStockRepos.UpdateDispatchWarehouseStockAsync(dispatch, staffId, confirmedIds);
            }

            // 5. Lưu audit log và cập nhật transfer
            await _dispatchRepos.SaveChangesAsync();
            await UpdateTransferStatusAsync(dispatch.DispatchId, staffId);
        }

        private void ValidateDispatchStatus(Dispatch dispatch)
        {
            var allowed = new[] { "Approved", "Processing", "Done" };
            if (!allowed.Contains(dispatch.Status, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa đơn đã Approved hoặc Processing");
        }

        private async Task UpdateDispatchStoreDetails(Dispatch dispatch, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            var accountId = await _staffRepos.GetAccountIdByStaffIdAsync(staffId);
            foreach (var detail in dispatch.DispatchDetails.SelectMany(d => d.StoreExportStoreDetails))
            {
                var dto = confirmations.FirstOrDefault(c => c.StoreDetailId == detail.DispatchStoreDetailId);
                if (dto == null) continue;

                detail.Status = "Success"; // mặc định Success đã trimmed
                detail.Comments = dto.Comment;
                detail.ActualQuantity = detail.AllocatedQuantity;

                _auditLogRepos.Add(new AuditLog
                {
                    TableName = "StoreExportStoreDetail",
                    RecordId = detail.DispatchStoreDetailId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = "Status->Success",
                    Comment = "Xuất kho thành công !"
                });
            }
        }

        private void MarkDispatchDone(Dispatch dispatch, int staffId)
        {
            dispatch.Status = "Done"; // giá trị "Done" đã trimmed
            dispatch.CompletedDate = DateTime.Now;
            var accountId = _staffRepos.GetAccountIdByStaffIdAsync(staffId).Result;
            _auditLogRepos.Add(new AuditLog
            {
                TableName = "Dispatch",
                RecordId = dispatch.DispatchId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.Now,
                ChangedBy = accountId,
                ChangeData = "Status->Done",
                Comment = "Đơn xuất hàng hoàn tất !"
            });
        }

        private async Task UpdateTransferStatusAsync(int dispatchId, int staffId)
        {
            var transfer = await _importRepos.GetTransferByImportIdAsync(dispatchId);
            if (transfer == null) return;

            var dispDone = transfer.Dispatch?.Status?.Trim().Equals("Done", StringComparison.OrdinalIgnoreCase) == true;
            var impDone = transfer.Import?.Status?.Trim().Equals("Done", StringComparison.OrdinalIgnoreCase) == true;
            if (dispDone && impDone && transfer.Status != "Done")
            {
                transfer.Status = "Done";
                var accountId = await _staffRepos.GetAccountIdByStaffIdAsync(staffId);
                _auditLogRepos.Add(new AuditLog
                {
                    TableName = "Transfer",
                    RecordId = transfer.TransferOrderId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = "Status->Done",
                    Comment = "Chuyển hàng hoàn tất"
                });
                await _dispatchRepos.SaveChangesAsync();
                await _auditLogRepos.SaveChangesAsync();
            }
        }
    }
}
