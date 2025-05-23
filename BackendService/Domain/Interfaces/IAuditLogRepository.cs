using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAuditLogAsync(string tableName, string recordId, string operation, int changedBy, string? changeData, string? comment);
        Task<Dictionary<int, DateTime>> GetLatestDeliveredDatesAsync(List<int> orderIds);

        Task AddAsync(AuditLog auditLog);


        Task SaveChangesAsync();

        Task<IEnumerable<AuditLog>> GetByRecordIdAsync(string tableName, string recordId);


        Task<IEnumerable<AuditLog>> GetByUserAsync(int changedBy);

        Task<List<AuditLog>> GetAuditLogsByTableAndRecordIdAsync(string tableName, string recordId);

        void Add(AuditLog auditLog);
    }

}
