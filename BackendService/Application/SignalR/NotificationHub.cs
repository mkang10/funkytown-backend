using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace Application.SignalR
{
    public class NotificationHub : Hub
    {
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            var accountId = Context.User?.FindFirst("accountId")?.Value;

            Console.WriteLine($"[SignalR] ✅ Client connected | Extracted AccountId: {accountId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("❌ Client disconnected");

            await base.OnDisconnectedAsync(exception);
        }

        // 👉 Optional nếu muốn cho client gửi noti lên (đa số backend sẽ gửi nên phần này ít dùng)
        public async Task SendNotification(string accountId, string title, string message)
        {
            Console.WriteLine($"📢 SendNotification called | To AccountId={accountId}, Title={title}");

            await Clients.User(accountId).SendAsync("ReceiveNotification", title, message);
        }
    }
}
