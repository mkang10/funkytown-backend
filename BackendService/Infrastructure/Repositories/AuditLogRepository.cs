using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly FtownContext _context;

        public AuditLogRepository(FtownContext context)
        {
            _context = context;
        }


        public async Task AddAuditLogAsync(string tableName, string recordId, string operation, int changedBy, string? changeData, string? comment)
        {
            // ✅ Validate ChangedBy trước
            bool accountExists = await _context.Accounts.AnyAsync(a => a.AccountId == changedBy);

            if (!accountExists)
            {
                changedBy = 1 ; // Gán về SystemAccountId nếu account không tồn tại
            }

            var auditLog = new AuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                Operation = operation,
                ChangeDate = DateTime.UtcNow,
                ChangedBy = changedBy,
                ChangeData = changeData,
                Comment = comment
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        public async Task<Dictionary<int, DateTime>> GetLatestDeliveredDatesAsync(List<int> orderIds)
        {
            return await _context.AuditLogs
                .Where(al => al.TableName == "Orders" && al.Operation == "delivered" && orderIds.Contains(int.Parse(al.RecordId)))
                .GroupBy(al => al.RecordId)
                .Select(g => new
                {
                    OrderId = int.Parse(g.Key),
                    DeliveredDate = g.Max(al => al.ChangeDate)
                })
                .ToDictionaryAsync(x => x.OrderId, x => x.DeliveredDate);
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
        }

        /// <summary>
        /// Lưu thay đổi vào database.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy danh sách audit log theo bảng và ID bản ghi.
        /// </summary>
        public async Task<IEnumerable<AuditLog>> GetByRecordIdAsync(string tableName, string recordId)
        {
            return await _context.AuditLogs
                .Where(log => log.TableName == tableName && log.RecordId == recordId).Include(t => t.ChangedByNavigation)
                .OrderByDescending(log => log.ChangeDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách audit log theo user thực hiện.
        /// </summary>
        public async Task<IEnumerable<AuditLog>> GetByUserAsync(int changedBy)
        {
            return await _context.AuditLogs
                .Where(log => log.ChangedBy == changedBy)
                .OrderByDescending(log => log.ChangeDate)
                .ToListAsync();
        }

        public void Add(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
        }
        public async Task<List<AuditLog>> GetAuditLogsByTableAndRecordIdAsync(string tableName, string recordId)
        {
            return await _context.AuditLogs
                .Where(a => a.TableName == tableName && a.RecordId == recordId).Include(t => t.ChangedByNavigation)
                .ToListAsync();
        }
    }
}
