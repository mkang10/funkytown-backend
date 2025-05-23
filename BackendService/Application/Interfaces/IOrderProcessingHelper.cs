using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
	public interface IOrderProcessingHelper
	{
		Task SavePaymentAndOrderDetailsAsync(Order order, List<OrderDetail> orderDetails, string paymentMethod, decimal totalAmount, decimal shippingCost, long? orderCode = null);
		Task ClearCartAsync(int accountId, List<int> productVariantIds);
		Task LogPendingConfirmedStatusAsync(int orderId, int accountId);
		Task LogPendingPaymentStatusAsync(int orderId, int accountId);
		Task LogPendingReturnStatusAsync(int returnOrderId, int accountId);
		Task LogCancelStatusAsync(int returnOrderId, int accountId);
        OrderResponse BuildOrderResponse(Order order, string paymentMethod, string? paymentUrl = null);
		Task AssignOrderToManagerAsync(int orderId, int assignedBy);
		Task SendOrderNotificationAsync(int accountId, int orderId, string title, string message);
		Task SendReturnOrderNotificationAsync(int accountId, int returnOrderId, string title, string message);
		Task LogWarehouseStockChangeAsync(
										int orderId,
										int accountId,
										List<OrderDetail> orderDetails,
										int warehouseId);
		Task UpdateDefaultAssignmentAsync(int shopManagerId, int staffId);
		Task LogDeliveredStatusAsync(int orderId, int accountId);
		Task LogDeliveringStatusAsync(int orderId, int accountId);
		Task AssignReturnOrderToManagerAsync(int orderId, int assignedBy);
    }
}
