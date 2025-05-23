using Application.Enums;
using Application.Interfaces;
using Application.UseCases;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
    public class OrderAutoCompletionHandler : IOrderAutoCompletionHandler
    {
        private readonly FtownContext _context;
        private readonly AuditLogHandler _auditLogHandler;

        public OrderAutoCompletionHandler(FtownContext context, AuditLogHandler auditLogHandler)
        {
            _context = context;
            _auditLogHandler = auditLogHandler;
        }

        public async Task ProcessAutoCompleteOrdersAsync()
        {
            Console.WriteLine("[BGService] >>> Bắt đầu chạy ProcessAutoCompleteOrdersAsync <<<");
            var now = DateTime.UtcNow;

            // 1. Lấy tất cả log "Delivered" hơn 1 phút
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

            var deliveredLogs = await _context.AuditLogs
                .Where(x => x.Operation == AuditOperation.UpdateStatus.ToString()
                    && x.TableName == "Orders"
                    && x.ChangeDate <= oneMinuteAgo)
                .ToListAsync();
            Console.WriteLine($"[BGService] Đã lấy xong deliveredLogs, count = {deliveredLogs.Count}");
            if (deliveredLogs.Count == 0) return;

            var orderIds = deliveredLogs
                .Where(x => int.TryParse(x.RecordId, out _)) 
                .Select(x => int.Parse(x.RecordId))
                .Distinct()
                .ToList();
            Console.WriteLine($"[BGService] Số lượng deliveredLogs tìm được: {deliveredLogs.Count}");
            Console.WriteLine($"[BGService] Số lượng orderIds parse được: {orderIds.Count}");
            // 2. Lấy đơn hàng tương ứng
            var orders = await _context.Orders
                .Where(x => orderIds.Contains(x.OrderId) && x.Status == "Delivered")
                .ToListAsync();
            Console.WriteLine($"[BGService] Số lượng orders Delivered tìm được: {orders.Count}");
            foreach (var order in orders)
            {
                Console.WriteLine($"[BGService] Đang cập nhật OrderId: {order.OrderId} sang Completed.");
                order.Status = "Completed";
                order.CompletedDate = DateTime.UtcNow;
                // 3. Ghi thêm AuditLog
                await _auditLogHandler.LogOrderActionAsync(
                    order.OrderId,
                    AuditOperation.UpdateStatus,
                    new
                    {
                        From = "Delivered",
                        To = "Completed"
                    },
                    -1, // systemAccountId
                    "Hệ thống tự động chuyển đơn hàng sang Completed sau 48h kể từ khi Delivered."
                );
            }

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"[BGService] Đã SaveChangesAsync thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BGService] Lỗi SaveChangesAsync: {ex.Message}");
            }
        }
    }

}
