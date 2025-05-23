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
    public class UpdateReturnOrderStatusHandler
    {
        private readonly IReturnOrderRepository _returnOrderRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IOrderProcessingHelper _orderProcessingHelper;
        private readonly ILogger<UpdateReturnOrderStatusHandler> _logger;
        private readonly IOrderRepository _orderRepository;

        public UpdateReturnOrderStatusHandler(
            IReturnOrderRepository returnOrderRepository,
            IAuditLogRepository auditLogRepository,
            IOrderProcessingHelper orderProcessingHelper,
            ILogger<UpdateReturnOrderStatusHandler> logger,
            IOrderRepository orderRepository)
        {
            _returnOrderRepository = returnOrderRepository;
            _auditLogRepository = auditLogRepository;
            _orderProcessingHelper = orderProcessingHelper;
            _logger = logger;
            _orderRepository = orderRepository;
        }

        public async Task<bool> HandleAsync(int returnOrderId, string newStatus, int changedBy, string? comment)
        {
            var returnOrder = await _returnOrderRepository.GetByIdAsync(returnOrderId);
            if (returnOrder == null)
            {
                _logger.LogWarning($"[UpdateReturnOrderStatus] ReturnOrderId {returnOrderId} không tồn tại.");
                return false;
            }

            var previousStatus = returnOrder.Status;

            // 1️⃣ Cập nhật trạng thái ReturnOrder
            returnOrder.Status = newStatus;
            returnOrder.UpdatedDate = DateTime.Now;
            await _returnOrderRepository.UpdateAsync(returnOrder);

            // 2️⃣ Cập nhật trạng thái Order nếu cần
            if (newStatus == "Approved" || newStatus == "Rejected")
            {
                var order = await _orderRepository.GetOrderByIdAsync(returnOrder.OrderId);
                if (order != null && order.Status == "Return Requested")
                {
                    var newOrderStatus = newStatus == "Approved" ? "Return Approved" : "Return Rejected";
                    await _orderRepository.UpdateOrderStatusAsync(order.OrderId, newOrderStatus);
                }
            }

            // 3️⃣ Ghi log thay đổi trạng thái ReturnOrder
            var changeData = JsonSerializer.Serialize(new
            {
                OldStatus = previousStatus,
                NewStatus = newStatus
            });

            await _auditLogRepository.AddAuditLogAsync(
                "ReturnOrders",
                returnOrderId.ToString(),
                newStatus,
                changedBy,
                changeData,
                comment
            );

            // 4️⃣ Gửi thông báo đến người dùng
            try
            {
                var message = newStatus switch
                {
                    "Approved" => $"Yêu cầu đổi/trả đơn hàng #{returnOrder.OrderId} của bạn đã được phê duyệt.",
                    "Rejected" => $"Yêu cầu đổi/trả đơn hàng #{returnOrder.OrderId} của bạn đã bị từ chối.",
                    _ => $"Yêu cầu đổi/trả đơn hàng #{returnOrder.OrderId} đã được cập nhật trạng thái: {newStatus}."
                };

                await _orderProcessingHelper.SendReturnOrderNotificationAsync(
                    returnOrder.AccountId,
                    returnOrder.ReturnOrderId,
                    "Cập nhật trạng thái đổi/trả đơn hàng",
                    message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"[NotifyError] Gửi thông báo thất bại: {ex.Message}");
            }

            return true;
        }

    }

}
