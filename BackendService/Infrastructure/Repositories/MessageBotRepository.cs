using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MessageBotRepository : IMessageBotRepository
    {
        private readonly FtownContext _ctx;
        public MessageBotRepository(FtownContext ctx) => _ctx = ctx;

        public async Task AddAsync(MessagesBot message, CancellationToken ct = default)
        {
            await _ctx.MessagesBots.AddAsync(message, ct);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<MessagesBot>> GetByConversationIdAsync(int conversationId, CancellationToken ct = default)
            => await _ctx.MessagesBots
                         .Where(m => m.ConversationId == conversationId)
                         .OrderBy(m => m.SentAt)
                         .ToListAsync(ct);
    }
}
