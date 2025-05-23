using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ImportIncompletedHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IAuditLogRepository _auditLogRepos;

        public ImportIncompletedHandler(IImportRepos importRepos, IAuditLogRepository auditLogRepos)
        {
            _importRepos = importRepos;
            _auditLogRepos = auditLogRepos;
        }


        public async Task ProcessImportIncompletedAsync(int importId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {
            // Lấy Import đã được include các ImportDetails và ImportStoreDetails
            var import = await _importRepos.GetByIdAssignAsync(importId);
            if (import == null)
            {
                throw new Exception("Import không tồn tại");
            }

            if (!string.Equals(import.Status, "Processing", StringComparison.OrdinalIgnoreCase) &&
     !string.Equals(import.Status, "Partial Success", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Processing hoặc Partial Success");
            }


            // 1. Cập nhật từng ImportStoreDetail về trạng thái "Failed" dựa trên confirmations
            UpdateStoreDetailsToFailed(import, confirmations, staffId);

            // 2. Nếu có bất kỳ ImportStoreDetail nào có status "Failed", cập nhật Import tổng với status "Failed"
            if (AnyStoreDetailFailed(import))
            {
                UpdateImportStatusToFailed(import, staffId);
            }

            // Lưu các thay đổi
            await _importRepos.SaveChangesAsync();
            await _auditLogRepos.SaveChangesAsync();
        }

        /// <summary>
        /// Duyệt qua từng ImportDetail và ImportStoreDetail, cập nhật status thành "Failed" nếu có xác nhận.
        /// Tạo AuditLog cho mỗi ImportStoreDetail được cập nhật.
        /// </summary>
        private void UpdateStoreDetailsToFailed(Import import, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            foreach (var importDetail in import.ImportDetails)
            {
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    // Tìm confirmation dựa trên ImportStoreDetailId
                    var confirmation = confirmations.FirstOrDefault(c => c.StoreDetailId == storeDetail.ImportStoreId);
                    if (confirmation == null)
                    {
                        continue;
                    }

                    // Cập nhật trạng thái "Failed" và ghi chú
                    storeDetail.Status = "Failed";
                    storeDetail.Comments = string.IsNullOrEmpty(confirmation.Comment)
                                            ? "Hàng không đủ"
                                            : confirmation.Comment;

                    // Tạo bản ghi AuditLog cho cập nhật ImportStoreDetail
                    var auditLogDetail = new AuditLog
                    {
                        TableName = "ImportStoreDetail",
                        RecordId = storeDetail.ImportStoreId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = staffId,
                        ChangeData = "Status updated to Failed",
                        Comment = storeDetail.Comments
                    };
                    _auditLogRepos.Add(auditLogDetail);
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem có bất kỳ ImportStoreDetail nào có status "Failed" không.
        /// </summary>
        private bool AnyStoreDetailFailed(Import import)
        {
            return import.ImportDetails.Any(detail =>
                        detail.ImportStoreDetails.Any(sd =>
                            string.Equals(sd.Status, "Failed", StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Cập nhật Import thành "Failed" và tạo AuditLog cho Import.
        /// </summary>
        private void UpdateImportStatusToFailed(Import import, int staffId)
        {
            import.Status = "Failed";
            import.CompletedDate = DateTime.Now;

            var auditLogImport = new AuditLog
            {
                TableName = "Import",
                RecordId = import.ImportId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.Now,
                ChangedBy = staffId,
                ChangeData = "Status updated to Failed",
                Comment = "At least one ImportStoreDetail has status Failed"
            };
            _auditLogRepos.Add(auditLogImport);
        }

    }

}