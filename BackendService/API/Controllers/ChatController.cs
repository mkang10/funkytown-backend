using Domain.DTO.Request;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatAppService _svc;
        public ChatController(ChatAppService svc) => _svc = svc;

        [HttpPost("message")]
        public async Task<IActionResult> Message(
            [FromBody] ChatMessageRequest req,
            CancellationToken ct)
        {
            var reply = await _svc.GetFullReplyAsync(req.UserId, req.Content, ct);
            var convId = await _svc.GetOrCreateConversationAsync(req.UserId, ct);
            return Ok(new { conversationId = convId, reply });
        }
    }
}
