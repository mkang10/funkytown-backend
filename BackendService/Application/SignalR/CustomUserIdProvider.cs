using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.SignalR
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"[SignalR] Extracted userId (AccountId): {userId}");

            return userId; // sẽ là accountId ("7")
        }
    }


}
