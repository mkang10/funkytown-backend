using API.Chathub;
using Application.Enum;
using Application.Interfaces;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _service;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(IMessageService service, IHubContext<ChatHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [HttpGet("getAll/{id}")]
        public async Task<IActionResult> GetAllMessage(int id, [FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _service.GetAllMessageServiceByConversationId(id, paginationParameter);

                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString());
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
        public async Task<IActionResult> CreateMessage(MessageCreateRequest user)
        {
            try
            {
                var data = await _service.createMessage(user);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", user.SenderId, user.MessageContent);

                if (data == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }

                // Gửi tin nhắn tới tất cả người dùng qua SignalR
                return Ok(new MessageRespondDTO<MessageCreateRequest>(data, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {

            try
            {
                var result = await _service.deleteMessage(id);
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
    }
}
