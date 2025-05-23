using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases
{
    public class ApproveHandler
    {
        private readonly IImportRepos _repository;
        private readonly IAuditLogRepository _auditLogRepos;

        public ApproveHandler(IImportRepos repository, IAuditLogRepository auditLogRepos)
        {
            _repository = repository;
            _auditLogRepos = auditLogRepos;
        }

        public async Task ApproveImportAsync(int importId, int changedBy, string? comments)
        {
            // Lấy đơn import theo ID
            var import = await _repository.GetByIdAsync(importId);
            if (import == null)
                throw new Exception($"Không tìm thấy đơn nhập hàng với ID = {importId}.");

            // Kiểm tra trạng thái: chỉ xử lý nếu đơn đang "pending"
            if (!string.Equals(import.Status?.Trim(), "pending", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Chỉ có thể duyệt đơn nhập hàng ở trạng thái 'pending'.");

            // Cập nhật trạng thái
            import.Status = "Approved";
            import.ApprovedDate = DateTime.Now;
            import.CompletedDate = null; // Chưa hoàn thành

            // Cập nhật vào repository
            await _repository.UpdateAsync(import);

            // Ghi nhận vào AuditLog
            var serializedChangeData = JsonConvert.SerializeObject(import, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var auditLog = new AuditLog
            {
                TableName = "Import",
                RecordId = import.ImportId.ToString(),
                Operation = "APPROVE",
                ChangeDate = DateTime.Now,
                ChangedBy = changedBy,
                ChangeData = serializedChangeData,
                Comment = comments ?? "Đơn nhập hàng được phê duyệt"
            };

            _auditLogRepos.AddAsync(auditLog);
            await _auditLogRepos.SaveChangesAsync();
        }
    }
}
