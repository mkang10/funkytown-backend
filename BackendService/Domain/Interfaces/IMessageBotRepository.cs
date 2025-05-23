using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IMessageBotRepository
    {
        Task<IEnumerable<MessagesBot>> GetByConversationIdAsync(int conversationId, CancellationToken ct = default);
        Task AddAsync(MessagesBot message, CancellationToken ct = default);
    }
}
