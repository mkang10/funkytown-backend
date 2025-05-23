
using Application.Interfaces;
using Application.UseCases;
using Domain.Common_Model;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly GetOrderHandler _getOrderHandler;
        private readonly CompletedOrderHandler _completedOrder;

        private readonly AssignStaffHandler _assign;
        private readonly CreateOrderHandler _createOrderHandler;
        private readonly ILogger<OrderController> _logger;
        private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
        private readonly GetOrdersByStatusHandler _getOrdersByStatusHandler;
        private readonly GetOrderDetailHandler _getOrderDetailHandler;
        private readonly GetOrderItemsHandler _getOrderItemsHandler;
        private readonly GetReturnableOrdersHandler _getReturnableOrdersHandler;
        private readonly IOrderProcessingHelper _orderProcessingHelper;
        public OrderController(CreateOrderHandler createOrderHandler, 
                               ILogger<OrderController> logger, 
                               GetOrdersByStatusHandler getOrdersByStatusHandler, 
                               GetOrderDetailHandler getOrderDetailHandler, 
                               GetOrderItemsHandler getOrderItemsHandler, 
                               UpdateOrderStatusHandler updateOrderStatusHandler,
                               GetReturnableOrdersHandler getReturnableOrdersHandler,
                               IOrderProcessingHelper orderProcessingHelper,
                               CompletedOrderHandler completedOrder, AssignStaffHandler assign, GetOrderHandler getOrderHandler)
        {
            _createOrderHandler = createOrderHandler;
            _logger = logger;
            _updateOrderStatusHandler = updateOrderStatusHandler;
            _getOrdersByStatusHandler = getOrdersByStatusHandler;
            _getOrderDetailHandler = getOrderDetailHandler;
            _getOrderItemsHandler = getOrderItemsHandler;
            _getReturnableOrdersHandler = getReturnableOrdersHandler;
            _orderProcessingHelper = orderProcessingHelper;
            _getOrderHandler = getOrderHandler;
            _assign = assign;
            _completedOrder = completedOrder;
        }

        /// <summary>
        /// 📌 Tạo đơn hàng (COD hoặc PAYOS)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<OrderResponse>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Lấy các lỗi từ ModelState
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                // Trả về ResponseDTO với trạng thái false và thông báo lỗi
                return BadRequest(new ResponseDTO<OrderResponse>(null, false, "Dữ liệu không hợp lệ."));
            }

            try
            {
                var result = await _createOrderHandler.Handle(request);
                if (result == null)
                {
                    _logger.LogWarning("Tạo đơn hàng thất bại cho AccountId: {AccountId}", request.AccountId);
                    return BadRequest(new ResponseDTO<OrderResponse>(null, false, "Tạo đơn hàng thất bại. Vui lòng thử lại sau."));
                }

                return Ok(new ResponseDTO<OrderResponse>(result, true, "Tạo đơn hàng thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi xảy ra khi tạo đơn hàng cho AccountId: {AccountId}", request.AccountId);
                return StatusCode(500, new ResponseDTO<OrderResponse>(null, false, "Có lỗi xảy ra trong quá trình tạo đơn hàng."));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO<PaginatedResult<OrderResponse>>>> GetOrdersByStatus(
                                                                                        [FromQuery] string? status,
                                                                                        [FromQuery] int? accountId = null,
                                                                                        [FromQuery] int pageNumber = 1,
                                                                                        [FromQuery] int pageSize = 10)
        {
            var pagedOrders = await _getOrdersByStatusHandler.HandleAsync(status, accountId, pageNumber, pageSize);
            return Ok(new ResponseDTO<PaginatedResult<OrderResponse>>(
                pagedOrders,
                true,
                $"Danh sách đơn hàng với trạng thái {status} {(accountId.HasValue ? $"và accountId {accountId}" : "")} được lấy thành công."));
        }

        [HttpGet("returnable")]
        public async Task<ActionResult<ResponseDTO<List<OrderResponse>>>> GetReturnableOrders([FromQuery] int accountId)
        {
            var orders = await _getReturnableOrdersHandler.HandleAsync(accountId);
            return Ok(new ResponseDTO<List<OrderResponse>>(orders, true, "Danh sách đơn hàng có thể hoàn trả được lấy thành công."));
        }

        [HttpGet("{orderId}/details")]
        public async Task<IActionResult> GetOrderDetails(int orderId, [FromQuery] int accountId)
        {
            var orderDetailResponse = await _getOrderDetailHandler.HandleAsync(orderId, accountId);
            if (orderDetailResponse == null)
            {
                return NotFound(new ResponseDTO<OrderDetailResponseWrapper>(null, false, "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập."));
            }

            return Ok(new ResponseDTO<OrderDetailResponseWrapper>(orderDetailResponse, true, "Lấy chi tiết đơn hàng thành công!"));
        }


        [HttpGet("{orderId}/items")]
        public async Task<IActionResult> GetOrderItemsById(int orderId)
        {
            var result = await _getOrderItemsHandler.HandleAsync(orderId);

            if (result == null || !result.Any())
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong đơn hàng." });
            }

            return Ok(result);
        }

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new ResponseDTO<Dictionary<string, string[]>>(errors, false, "Dữ liệu không hợp lệ"));
            }

            var success = await _updateOrderStatusHandler.HandleAsync(
                orderId,
                request.NewStatus,
                request.ChangedBy,
                request.Comment
            );

            if (!success)
            {
                return NotFound(new ResponseDTO(false, "Không tìm thấy đơn hàng."));
            }

            return Ok(new ResponseDTO(true, "Cập nhật trạng thái thành công!"));
        }

        [HttpPost("assignment/defaults")]
        public async Task<IActionResult> UpdateDefaultAssignment([FromBody] UpdateAssignmentSettingRequest request)
        {
            await _orderProcessingHelper.UpdateDefaultAssignmentAsync(request.ShopManagerId, request.StaffId);
            return Ok(new { Success = true, Message = "Cập nhật thành công!" });
        }

        [HttpGet("{orderId}/is-returnable")]
        public async Task<ActionResult<ResponseDTO<bool>>> IsOrderReturnable(int orderId, [FromQuery] int accountId)
        {
            var result = await _getReturnableOrdersHandler.CheckReturnableHandleAsync(orderId, accountId);
            return Ok(new ResponseDTO<bool>(result, true, "Kiểm tra điều kiện đổi trả thành công."));
        }

        [HttpGet("assignment")]
        public async Task<ActionResult<PaginatedResponseDTO<OrderAssignmentDto>>> GetAll(
       [FromQuery] OrderAssignmentFilterDto filter,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10)
        {
            var result = await _getOrderHandler.GetAllAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPut("assign")]
        public async Task<IActionResult> AssignStaff([FromBody] AssignStaffDTO dto)
        {
            var result = await _assign.AssignStaffAsync(dto);
            return result.Status ? Ok(result) : NotFound(result);
        }

        [HttpPut("{orderId}/complete")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var result = await _completedOrder.CompleteOrderAsync(orderId);
            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{assignmentId}")]
        public async Task<ActionResult<OrderAssignmentDto>> GetById(int assignmentId)
        {
            var dto = await _getOrderHandler.GetByIdAsync(assignmentId);
            if (dto == null)
                return NotFound(new { Message = $"OrderAssignment {assignmentId} không tồn tại." });

            return Ok(dto);
        }


    }
}
