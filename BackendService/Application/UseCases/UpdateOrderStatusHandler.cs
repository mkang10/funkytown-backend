using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class UpdateOrderStatusHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient; // Gọi API kho

        public UpdateOrderStatusHandler(
            IOrderRepository orderRepository,
            IAuditLogRepository auditLogRepository,
            IInventoryServiceClient inventoryServiceClient)
        {
            _orderRepository = orderRepository;
            _auditLogRepository = auditLogRepository;
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<bool> HandleAsync(int orderId, string newStatus, int changedBy, string? comment)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null) return false;

            var previousStatus = order.Status;

            if (newStatus == "Canceled")
            {
                var restoreSuccess = await _inventoryServiceClient.RestoreStockAfterCancelAsync(
                    2,
                    order.OrderDetails.ToList()
                );
                if (!restoreSuccess)
                {
                    Console.WriteLine("[ERROR] Không thể khôi phục tồn kho khi huỷ đơn.");
                    return false;
                }
            }

            if (newStatus.ToLowerInvariant() == "Completed" && order.CompletedDate == null)
            {
                order.CompletedDate = DateTime.Now;
            }

            await _orderRepository.UpdateOrderStatusWithOrderAsync(order, newStatus);


            var changeData = JsonSerializer.Serialize(new
            {
                OldStatus = previousStatus,
                NewStatus = newStatus
            });

            await _auditLogRepository.AddAuditLogAsync(
                "Orders",
                orderId.ToString(),
                newStatus,
                changedBy,
                changeData,
                comment
            );

            return true;
        }

    }


}
