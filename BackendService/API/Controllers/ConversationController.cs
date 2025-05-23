using Application.Enum;
using Application.Interfaces;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

internal class T
{
}

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _service;

        public ConversationController(IConversationService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUserAccount(int id, [FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _service.GetAllConversationServiceByAccountId(id, paginationParameter);

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
        public async Task<IActionResult> CreateConversation(ConversationCreateRequest user)
        {
            try
            {
                var data = await _service.createConversation(user);
                if (data == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }
                return Ok(new MessageRespondDTO<ConversationCreateRequest>(data, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteConversationById(int id)
        {

            try
            {
                var result = await _service.deleteConversation(id);
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
