using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IConversationBotRepository
    {
        Task<ConversationsBot?> GetByIdAsync(int conversationId, CancellationToken ct = default);
        Task<IEnumerable<ConversationsBot>> GetByUserAsync(int userId, CancellationToken ct = default);
        Task AddAsync(ConversationsBot conversation, CancellationToken ct = default);
    }
}
