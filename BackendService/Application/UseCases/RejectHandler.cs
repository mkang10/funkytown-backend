using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Domain.Entities;
using Domain.DTO.Response;
using Domain.DTO.Request;
using Domain.Interfaces;

namespace Application.UseCases
{
    public class RejectHandler
    {
        private readonly IImportRepos _repository;
        private readonly IAuditLogRepository _auditLogRepos;

        public RejectHandler(IImportRepos repository, IAuditLogRepository auditLogRepos)
        {
            _repository = repository;
            _auditLogRepos = auditLogRepos;
        }

        public async Task RejectImportAsync(int importId, int changedBy, string? comments)
        {
            // Lấy đơn import theo ID
            var import = await _repository.GetByIdAsync(importId);
            if (import == null)
                throw new Exception($"Không tìm thấy đơn nhập hàng với ID = {importId}.");

            // Kiểm tra trạng thái: chỉ từ chối đơn đang ở trạng thái "pending"
            if (!string.Equals(import.Status?.Trim(), "pending", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Chỉ có thể từ chối đơn nhập hàng đang ở trạng thái 'pending'.");

            // Cập nhật trạng thái và hoàn thành đơn import
            import.Status = "Rejected";
            import.CompletedDate = DateTime.Now;

            // Cập nhật lại đơn import qua repository
            await _repository.UpdateAsync(import);

            // Tạo AuditLog: thay vì serialize toàn bộ entity (có thể gây vòng lặp),
            // ta cấu hình serialize với ReferenceLoopHandling.Ignore
            var serializedChangeData = JsonConvert.SerializeObject(import, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var auditLog = new AuditLog
            {
                TableName = "Import",
                RecordId = import.ImportId.ToString(),
                Operation = "REJECT",
                ChangeDate = DateTime.Now,
                ChangedBy = changedBy,
                ChangeData = serializedChangeData,
                Comment = comments ?? "Đơn nhập hàng bị từ chối"
            };

            _auditLogRepos.AddAsync(auditLog);
            await _auditLogRepos.SaveChangesAsync();
        }
    }
}
