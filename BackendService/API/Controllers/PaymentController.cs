using Application.Interfaces;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        
        private readonly ILogger<PaymentController> _logger;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IInventoryServiceClient _inventoryServiceClient;
		private readonly IOrderProcessingHelper _orderProcessingHelper;
		public PaymentController(ILogger<PaymentController> logger,
								 IOrderRepository orderRepository,
								 IPaymentRepository paymentRepository,
								 IInventoryServiceClient inventoryServiceClient,
								 IOrderProcessingHelper orderProcessingHelper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
			_inventoryServiceClient = inventoryServiceClient;
			_orderProcessingHelper = orderProcessingHelper;
        }

		[HttpPost("webhook")]
		public async Task<IActionResult> PayOSWebhook([FromBody] PayOSCallbackRoot callbackData)
		{
			// Log để xem payload PayOS gửi
			_logger.LogInformation("Nhận callback từ PayOS: {@CallbackData}", callbackData);

			// Kiểm tra tính hợp lệ của callback (chữ ký, token, ...)
			if (!IsValidSignature(callbackData))
			{
				_logger.LogWarning("Callback từ PayOS không hợp lệ (chữ ký sai).");
				return BadRequest();
			}

			// Kiểm tra code hoặc status để xác định giao dịch có thành công không
			// - "code": "00" => thành công
			// - "success": true => thành công
			// - "status" (trong callbackData.data) = "success" => thành công
			if (callbackData.code == "00" && callbackData.success &&
				callbackData.data != null &&
				callbackData.data.desc == "success")
			{
                // Lấy mã đơn hàng từ callback
                long orderCode = callbackData.data.orderCode;

                // Tìm Payment/Order tương ứng trong DB
                var payment = await _paymentRepository.GetPaymentByOrderCodeAsync(orderCode);
                if (payment == null)
                {
                    _logger.LogError("Không tìm thấy Payment với OrderCode: {OrderCode}", orderCode);
                    return NotFound();
                }
                int orderId = payment.OrderId;

                // Cập nhật trạng thái Payment và Order
                payment.PaymentStatus = "Paid";
				await _paymentRepository.UpdatePaymentAsync(payment);

				await _orderRepository.UpdateOrderStatusAsync(orderId, "Paid");
				_logger.LogInformation("Cập nhật trạng thái đơn hàng {OrderId} thành Paid thành công.", orderId);
				

				var order = await _orderRepository.GetOrderByIdAsync(orderId);
				if (order == null)
				{
					_logger.LogError("Không tìm thấy Order: {OrderId}", orderId);
					return NotFound();
				}

				var orderDetails = order.OrderDetails.ToList();

				var updateStockSuccess = await _inventoryServiceClient.UpdateStockAfterOrderAsync((int)order.WareHouseId, orderDetails);
				if (!updateStockSuccess)
				{
					_logger.LogError("Cập nhật tồn kho thất bại cho OrderId: {OrderId}", orderId);
					return StatusCode(500, "Lỗi cập nhật tồn kho.");
				}
                await _orderProcessingHelper.LogWarehouseStockChangeAsync(orderId: order.OrderId, accountId: order.AccountId, orderDetails: orderDetails, warehouseId: (int)order.WareHouseId);
                await _orderProcessingHelper.LogPendingConfirmedStatusAsync(orderId, order.AccountId);
                await _orderProcessingHelper.AssignOrderToManagerAsync(orderId, order.AccountId);
				await _orderProcessingHelper.SendOrderNotificationAsync(
                                        order.AccountId,
                                        order.OrderId,
                                        "Đơn hàng mới",
                                        $"Đơn hàng #{order.OrderId} đã thanh toán thành công và đang chờ xác nhận."
                                    );
            }
			else
			{
				// Trường hợp giao dịch không thành công hoặc status khác
				_logger.LogWarning("Giao dịch không thành công hoặc status không phải success.");
			}

			// 7. Trả về 200 OK để PayOS biết bạn đã nhận callback
			return Ok();
		}
        [HttpGet("cancel")]
        public async Task<IActionResult> CancelPayment([FromQuery] long orderCode)
        {
            _logger.LogWarning("Yêu cầu huỷ thanh toán từ PayOS với OrderCode: {OrderCode}", orderCode);

            // Tìm bản ghi thanh toán theo orderCode
            var payment = await _paymentRepository.GetPaymentByOrderCodeAsync(orderCode);
            if (payment == null)
            {
                _logger.LogError("Không tìm thấy thanh toán nào với OrderCode: {OrderCode}", orderCode);
                return NotFound();
            }

            // Nếu đã Paid rồi thì không huỷ nữa
            if (payment.PaymentStatus == "Paid")
            {
                _logger.LogWarning("Không thể huỷ thanh toán vì đã ở trạng thái Paid.");
                return Redirect("https://ftown-client-pro-n8x7.vercel.app");
            }

            // Cập nhật trạng thái thanh toán
            payment.PaymentStatus = "Canceled";
            await _paymentRepository.UpdatePaymentAsync(payment);

            // Cập nhật trạng thái đơn hàng tương ứng
            var orderId = payment.OrderId;
            await _orderRepository.UpdateOrderStatusAsync(orderId, "Canceled");

            _logger.LogInformation("Huỷ thanh toán và đơn hàng thành công: OrderId={OrderId}, OrderCode={OrderCode}", orderId, orderCode);

            // Lấy order để ghi log hoặc gửi thông báo
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                await _orderProcessingHelper.LogCancelStatusAsync(orderId, order.AccountId);
                await _orderProcessingHelper.SendOrderNotificationAsync(
                    order.AccountId,
                    order.OrderId,
                    "Đơn hàng đã huỷ",
                    $"Bạn đã huỷ thanh toán cho đơn hàng #{order.OrderId}."
                );
            }

            // Điều hướng về frontend
            return Redirect("https://ftown-client-pro-n8x7.vercel.app");
        }

        private bool IsValidSignature(PayOSCallbackRoot callbackData)
		{
			// TODO: Triển khai logic kiểm tra chữ ký (signature) với secret/key 
			// mà PayOS cung cấp cho bạn
			return true;
		}
	}
}
