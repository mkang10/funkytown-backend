
using Application.Interfaces;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;



namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ICommentService _service;

        public FeedbackController(ICommentService service)
        {
            _service = service;
        }

        [HttpGet("accountid/{id}")]
        public async Task<IActionResult> GettAllFeedbackByAccountId(int id, [FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _service.GettAllFeedbackByAccountId(id, paginationParameter);

                if (result == null)
                {
                    var notFoundResponse = new ResponseDTO<object>(Array.Empty<object>(), true, StatusSuccess.Success.ToString());
                    return NotFound(notFoundResponse);
                }
                else
                {
                    var metadata = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    };

                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                }
                var successResponse = new MessageRespondDTO<object>(result, true, StatusSuccess.Success.ToString());
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(Array.Empty<object>(), true, "No data");
                return BadRequest(errorResponse);
            }
        }
        [HttpGet("productid/{id}")]
        public async Task<IActionResult> GetAllFeedbackByProductId(int id, [FromQuery] PaginationParameter paginationParameter)
        {
            // gọi service (không bao giờ throw nữa)
            var result = await _service.GetAllFeedbackByProductId(id, paginationParameter);

            if (result == null)
            {
                // có thể giữ NotFound nếu id sản phẩm không tồn tại
                return NotFound(new MessageRespondDTO<object>(Array.Empty<object>(), false, StatusSuccess.Wrong.ToString()));
            }

            // Thêm header X-Pagination
            var metadata = new
            {
                result.TotalCount,
                result.PageSize,
                result.CurrentPage,
                result.TotalPages,
                result.HasNext,
                result.HasPrevious
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));

            // Xác định message theo kết quả
            var message = result.TotalCount == 0
                ? "Chưa có đánh giá"
                : StatusSuccess.Success.ToString();

            var successResponse = new MessageRespondDTO<object>(result, true, message);
            return Ok(successResponse);
        }

        [HttpPost("create-multiple")]
        public async Task<IActionResult> CreateMultiple([FromForm] CreateMultipleFeedBackRequest feedbackRequest)
        {
            try
            {
                if (feedbackRequest?.Feedbacks == null || !feedbackRequest.Feedbacks.Any())
                {
                    return BadRequest(new MessageRespondDTO<string>(null, false, "Danh sách feedback không được để trống."));
                }

                var createdFeedbacks = await _service.CreateMultiple(feedbackRequest.Feedbacks);

                if (createdFeedbacks == null || !createdFeedbacks.Any())
                {
                    return BadRequest(new MessageRespondDTO<string>(null, false, "Không thể tạo feedback."));
                }

                return Ok(new MessageRespondDTO<List<FeedbackRequestDTO>>(createdFeedbacks, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<string>(null, false, ex.Message));
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateFeedBackRequestDTO feedbackRequest)
        {
            try
            {
                if (feedbackRequest == null)
                {
                    return BadRequest(new MessageRespondDTO<string>(null, false, "Danh sách feedback không được để trống."));
                }

                var createdFeedbacks = await _service.Create(feedbackRequest);

                if (createdFeedbacks == null)
                {
                    return BadRequest(new MessageRespondDTO<string>(null, false, "Không thể tạo feedback."));
                }

                return Ok(new MessageRespondDTO<FeedbackRequestDTO>(createdFeedbacks, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<string>(null, false, ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var response = new MessageResponseButNoData();

            try
            {
                var result = await _service.Delete(id);
                if (result)
                {
                    return Ok(new MessageRespondDTO<object>(null, true, StatusSuccess.Success.ToString()));

                }
                return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));

            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateFeedbackRequestDTO user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }

                bool isUpdated = await _service.Update(id, user);

                if (isUpdated)
                {
                    return Ok(new MessageRespondDTO<object>(null, true, StatusSuccess.Success.ToString()));
                }
                else
                {
                    return NotFound(new MessageRespondDTO<object>(null, false, "Wrong Id to update!"));
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, ex.Message);
                return BadRequest(errorResponse);
            }
        }


    }
}
