using Infrastructure;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace API.Chathub
{
    public class BotHub : Hub
    {
        private readonly ChatAppService _chatSvc;

        public BotHub(ChatAppService chatSvc)
        {
            _chatSvc = chatSvc;
        }

        public async Task SendMessage(int userId, string message)
        {
            // Echo user message
            await Clients.Caller.SendAsync("ReceiveMessage", "user", message);

            try
            {
                // Gọi service để lấy reply
                var reply = await _chatSvc.GetFullReplyAsync(userId, message, CancellationToken.None);

                // Gửi lại cho client
                await Clients.Caller.SendAsync("ReceiveMessage", "assistant", reply);
            }
            catch (Exception ex)
            {
                // Gửi lỗi cho client và log
                await Clients.Caller.SendAsync("ReceiveMessage", "assistant",
                    $"[Error from server] {ex.Message}");
                // Bạn có thể log thêm: 
                // _logger.LogError(ex, "Error in SendMessage");
            }
        }
    }

}