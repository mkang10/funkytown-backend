using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IChatServices
    {
        Task StreamChatAsync(
            List<ChatMessage> history,
            Func<string, Task> onChunk,
            CancellationToken ct = default);

        Task<string> GetFullChatReplyAsync(
            List<ChatMessage> history,
            CancellationToken ct = default);
    }
}
