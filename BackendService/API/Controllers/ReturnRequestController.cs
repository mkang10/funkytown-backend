
using Application.UseCases;
using Azure;
using Domain.Common_Model;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace API.Controllers
{
    [ApiController]
    [Route("api/return-requests")]
    public class ReturnRequestController : ControllerBase
    {
        private readonly GetOrderItemsForReturnHandler _getOrderItemsForReturnHandler;
        private readonly ProcessReturnCheckoutHandler _processReturnCheckoutHandler;
        private readonly SubmitReturnRequestHandler _submitReturnRequestHandler;
        private readonly GetAllReturnRequestsHandler _getAllReturnRequestsHandler;
        private readonly UpdateReturnOrderStatusHandler _updateReturnOrderStatusHandler;
        public ReturnRequestController(
            GetOrderItemsForReturnHandler getOrderItemsForReturnHandler,
            ProcessReturnCheckoutHandler processReturnCheckoutHandler,
            SubmitReturnRequestHandler submitReturnRequestHandler,
            GetAllReturnRequestsHandler getAllReturnRequestsHandler,
            UpdateReturnOrderStatusHandler updateReturnOrderStatusHandler)
        {
            _getOrderItemsForReturnHandler = getOrderItemsForReturnHandler;
            _processReturnCheckoutHandler = processReturnCheckoutHandler;
            _submitReturnRequestHandler = submitReturnRequestHandler;
            _getAllReturnRequestsHandler = getAllReturnRequestsHandler;
            _updateReturnOrderStatusHandler = updateReturnOrderStatusHandler;
        }

        /// <summary>
        /// API 1: Lấy danh sách sản phẩm trong Order để đổi trả và lưu vào Redis
        /// </summary>
        [HttpGet("order-items")]
        public async Task<ActionResult<ResponseDTO<List<OrderItemResponse>>>> GetOrderItemsForReturn(
            [FromQuery] int orderId, [FromQuery] int accountId)
        {
            if (orderId <= 0 || accountId <= 0)
            {
                return BadRequest(new ResponseDTO<List<OrderItemResponse>>(null, false, "OrderId hoặc AccountId không hợp lệ."));
            }

            try
            {
                var result = await _getOrderItemsForReturnHandler.Handle(orderId, accountId);
                if (result == null || !result.Any())
                {
                    return NotFound(new ResponseDTO<List<OrderItemResponse>>(null, false, "Không tìm thấy sản phẩm trong đơn hàng."));
                }

                return Ok(new ResponseDTO<List<OrderItemResponse>>(result, true, "Danh sách sản phẩm đã được lấy và lưu vào Redis."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<List<OrderItemResponse>>(null, false, $"Lỗi khi lấy sản phẩm: {ex.Message}"));
            }
        }

        /// <summary>
        /// API 2: Chọn sản phẩm từ Redis để tiến hành đổi trả (chỉ truyền ID và số lượng)
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] ReturnCheckOutRequest request)
        {
            if (request == null || request.SelectedItems == null || !request.SelectedItems.Any())
            {
                return BadRequest("Danh sách sản phẩm không hợp lệ.");
            }

            if (request.OrderId <= 0 || request.AccountId <= 0)
            {
                return BadRequest("OrderId hoặc AccountId không hợp lệ.");
            }

            var checkOutResponse = await _processReturnCheckoutHandler.Handle(request);
            if (checkOutResponse == null)
            {
                return BadRequest("Không thể thực hiện đổi trả. Vui lòng kiểm tra lại thông tin sản phẩm.");
            }

            return Ok(checkOutResponse);
        }

        [HttpPost("submit-return-request")]
        public async Task<IActionResult> SubmitReturnRequest([FromForm] SubmitReturnRequest request)
        {
            if (string.IsNullOrEmpty(request.ReturnCheckoutSessionId))
            {
                return BadRequest(new ResponseDTO<string>(null, false, "ReturnCheckoutSessionId không hợp lệ."));
            }


            var submitResponse = await _submitReturnRequestHandler.Handle(request);
            return submitResponse == null
                ? BadRequest(new ResponseDTO<string>(null, false, "Không thể tạo đơn đổi trả."))
                : Ok(new ResponseDTO<SubmitReturnResponse>(submitResponse, true, "Yêu cầu đổi trả đã được tạo thành công."));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(
                                                [FromQuery] string? status,
                                                [FromQuery] string? returnOption,
                                                [FromQuery] DateTime? dateFrom,
                                                [FromQuery] DateTime? dateTo,
                                                [FromQuery] int? orderId,
                                                [FromQuery] int? returnOrderId,
                                                [FromQuery] int handledBy,
                                                [FromQuery] int pageNumber = 1,
                                                [FromQuery] int pageSize = 10)
        {
            var pagedResult = await _getAllReturnRequestsHandler.HandleAsync(
                status,
                returnOption,
                dateFrom,
                dateTo,
                orderId,
                returnOrderId,
                handledBy,
                pageNumber,
                pageSize
            );

            var response = new ResponseDTO<PaginatedResult<ReturnRequestWrapper>>(
                pagedResult,
                true,
                "Lấy danh sách yêu cầu đổi trả thành công."
            );

            return Ok(response);
        }

        [HttpPut("{returnOrderId}/status")]
        public async Task<IActionResult> UpdateReturnOrderStatus(int returnOrderId, [FromBody] UpdateOrderStatusRequest request)
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

            var success = await _updateReturnOrderStatusHandler.HandleAsync(
                returnOrderId,
                request.NewStatus,
                request.ChangedBy,
                request.Comment
            );

            if (!success)
            {
                return NotFound(new ResponseDTO(false, "Không tìm thấy đơn đổi/trả."));
            }

            return Ok(new ResponseDTO(true, "Cập nhật trạng thái đơn đổi/trả thành công!"));
        }
    }
}
