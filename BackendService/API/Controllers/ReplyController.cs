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
    public class ReplyController : ControllerBase
    {
        private readonly IReplyFeedbackService _service;

        public ReplyController(IReplyFeedbackService service)
        {
            _service = service;
        }    
        [HttpGet("replyid/{id}")]
        public async Task<IActionResult> GettAllReplyByFeedbackId(int id, [FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _service.GettAllReplyByFeedbackId(id, paginationParameter);

                if (result == null)
                {
                    var notFoundResponse = new ResponseDTO<object>(null, false, StatusSuccess.Wrong.ToString());
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
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateReplyRequestDTO user)
        {
            try
            {
                var data = await _service.Create(user);
                if (data == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }
                return Ok(new MessageRespondDTO<CreateReplyRequestDTO>(data, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
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
        public async Task<IActionResult> Update(int id, UpdateReplyRequestDTO user)
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
