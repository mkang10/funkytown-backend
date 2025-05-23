
using Application.Enums;
using Application.Interfaces;
using Application.UseCases;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
	public class OrderProcessingHelper : IOrderProcessingHelper
	{
		private readonly IPaymentRepository _paymentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IWareHouseStockAuditRepository _warehouseStockAuditRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<OrderProcessingHelper> _logger;
		private readonly AuditLogHandler _auditLogHandler;
        private readonly INotificationClient _notificationClient;
        private readonly WareHouseStockAuditHandler _stockAuditHandler;
        private readonly IAssignmentSettingService _assignmentSettingService;
        public OrderProcessingHelper(
			IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            ICustomerServiceClient customerServiceClient,
            IMapper mapper,
            ILogger<OrderProcessingHelper> logger,
            AuditLogHandler auditLogHandler,
            INotificationClient notificationClient,
            WareHouseStockAuditHandler stockAuditHandler,
            IWareHouseStockAuditRepository warehouseStockAuditRepository,
            IAssignmentSettingService assignmentSettingService)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _customerServiceClient = customerServiceClient;
            _mapper = mapper;
            _logger = logger;
            _auditLogHandler = auditLogHandler;
            _notificationClient = notificationClient;
            _stockAuditHandler = stockAuditHandler;
            _warehouseStockAuditRepository = warehouseStockAuditRepository;
            _assignmentSettingService = assignmentSettingService;
        }

        public async Task SavePaymentAndOrderDetailsAsync(
                                                            Order order,
                                                            List<OrderDetail> orderDetails,
                                                            string paymentMethod,
                                                            decimal totalAmount,
                                                            decimal shippingCost,
                                                            long? orderCode = null
                                                        )
        {
            var payment = new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Pending",
                Amount = totalAmount + shippingCost,
                TransactionDate = DateTime.UtcNow,
                OrderCode = orderCode 
            };

            await _paymentRepository.SavePaymentAsync(payment);
            await _orderRepository.SaveOrderDetailsAsync(orderDetails);
            order.OrderDetails = orderDetails;
        }

        public async Task ClearCartAsync(int accountId, List<int> productVariantIds)
		{
			var success = await _customerServiceClient.ClearCartAfterOrderAsync(accountId, productVariantIds);
			if (!success)
			{
				_logger.LogWarning("Không thể xóa sản phẩm khỏi giỏ hàng sau khi đặt hàng. AccountId: {AccountId}", accountId);
			}
		}
        public async Task LogPendingConfirmedStatusAsync(int orderId, int accountId)
        {
            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.CreateOrder,
                new
                {
                    InitialStatus = OrderStatus.PendingConfirmed.ToString()
                },
                accountId,
                "Đặt hàng thành công và đang đợi xác nhận ."
            );
        }

        public async Task LogPendingPaymentStatusAsync(int orderId, int accountId)
        {
            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.CreateOrder,
                new
                {
                    InitialStatus = OrderStatus.PendingPayment.ToString()
                },
                accountId,
                "Đặt hàng thành công và đang đợi thanh toán."
            );
        }
        public async Task LogCancelStatusAsync(int returnOrderId, int accountId)
        {
            await _auditLogHandler.LogReturnOrderActionAsync(
                returnOrderId,
                AuditOperation.CancelOrder,
                new
                {
                    InitialStatus = OrderStatus.Canceled.ToString()
                },
                accountId,
                "Thanh toán đã bị hủy."
            );
        }
        public async Task LogPendingReturnStatusAsync(int returnOrderId, int accountId)
        {
            await _auditLogHandler.LogReturnOrderActionAsync(
                returnOrderId,
                AuditOperation.CreateReturnOrder,
                new
                {
                    InitialStatus = ReturnOrderStatus.Pending.ToString()
                },
                accountId,
                "Yêu cầu đổi/trả đã được tạo và đang chờ xử lý."
            );
        }
        public async Task LogDeliveredStatusAsync(int orderId, int accountId)
        {
            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.UpdateStatus, // Operation lần này là UpdateStatus (khác với CreateOrder lúc đặt)
                new
                {
                    From = OrderStatus.Delivering.ToString(),
                    To = OrderStatus.Delivered.ToString()
                },
                accountId,
                "Đơn hàng đã được giao thành công."
            );
        }
        public async Task LogDeliveringStatusAsync(int orderId, int accountId)
        {
            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.UpdateStatus,
                new
                {
                    From = OrderStatus.Confirmed.ToString(),
                    To = OrderStatus.Delivering.ToString()
                },
                accountId,
                "Đơn hàng đang được vận chuyển."
            );
        }
        public OrderResponse BuildOrderResponse(Order order, string paymentMethod, string? paymentUrl = null)
		{
			var response = _mapper.Map<OrderResponse>(order);
			response.PaymentMethod = paymentMethod;
			response.PaymentUrl = paymentUrl;
			return response;
		}
        public async Task AssignOrderToManagerAsync(int orderId, int assignedBy)
        {
            var assignment = new OrderAssignment
            {
                OrderId = orderId,
                ShopManagerId = _assignmentSettingService.DefaultShopManagerId,
                StaffId = _assignmentSettingService.DefaultStaffId,
                AssignmentDate = DateTime.UtcNow,
                Comments = "Tự động phân công đơn hàng cho Shop Manager và Staff mặc định."
            };

            await _orderRepository.CreateAssignmentAsync(assignment);

            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.AssignToManager,
                new
                {
                    ShopManagerID = _assignmentSettingService.DefaultShopManagerId,
                    StaffID = _assignmentSettingService.DefaultStaffId
                },
                assignedBy,
                "Đơn hàng được phân công cho Shop Manager và Staff mặc định."
            );
        }

        public async Task AssignReturnOrderToManagerAsync(int orderId, int assignedBy)
        {
            var assignment = new OrderAssignment
            {
                OrderId = orderId,
                ShopManagerId = _assignmentSettingService.DefaultShopManagerId,
                StaffId = _assignmentSettingService.DefaultStaffId,
                AssignmentDate = DateTime.UtcNow,
                Comments = "Tự động phân công xử lý đơn đổi trả cho Shop Manager và Staff mặc định."
            };

            await _orderRepository.CreateAssignmentAsync(assignment);

            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.AssignToManager,
                new
                {
                    ShopManagerID = _assignmentSettingService.DefaultShopManagerId,
                    StaffID = _assignmentSettingService.DefaultStaffId
                },
                assignedBy,
                "Đơn hàng đổi trả được phân công cho Shop Manager và Staff mặc định."
            );
        }
        public async Task SendOrderNotificationAsync(int accountId, int orderId, string title, string message)
        {
            var notificationRequest = new SendNotificationRequest
            {
                AccountId = accountId,
                Title = title,
                Message = message,
                NotificationType = "Order",
                TargetId = orderId,
                TargetType = "Order"
            };

            await _notificationClient.SendNotificationAsync(notificationRequest);
        }

        public async Task SendReturnOrderNotificationAsync(int accountId, int returnOrderId, string title, string message)
        {
            var notificationRequest = new SendNotificationRequest
            {
                AccountId = accountId,
                Title = title,
                Message = message,
                NotificationType = "ReturnOrder",
                TargetId = returnOrderId,
                TargetType = "ReturnOrder"
            };

            await _notificationClient.SendNotificationAsync(notificationRequest);
        }
        public async Task LogWarehouseStockChangeAsync(
                                        int orderId,
                                        int accountId,
                                        List<OrderDetail> orderDetails,
                                        int warehouseId)
        {
            var variantIds = orderDetails.Select(o => o.ProductVariantId).ToList();
            var stockMap = await _warehouseStockAuditRepository.GetWarehouseStockMapAsync(variantIds, warehouseId);

            foreach (var detail in orderDetails)
            {
                if (stockMap.TryGetValue(detail.ProductVariantId, out var warehouseStockId))
                {
                    await _stockAuditHandler.LogDecreaseStockAsync(
                        warehouseStockId: warehouseStockId,
                        quantityReduced: detail.Quantity,
                        changedBy: accountId,
                        note: $"Đơn hàng #{orderId} đã trừ {detail.Quantity} sản phẩm VariantId {detail.ProductVariantId}."
                    );
                }
                else
                {
                    // Ghi log lỗi nếu không tìm thấy kho
                    _logger.LogWarning($"Không tìm thấy WareHouseStock cho VariantId: {detail.ProductVariantId}, WareHouseId: {warehouseId}");
                }
            }
        }
        public async Task UpdateDefaultAssignmentAsync(int shopManagerId, int staffId)
        {
            _assignmentSettingService.UpdateDefaultAssignment(shopManagerId, staffId);
        }


    }

}
