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
    public class ConversationBotRepository : IConversationBotRepository
    {
        private readonly FtownContext _ctx;
        public ConversationBotRepository(FtownContext ctx) => _ctx = ctx;

        public async Task AddAsync(ConversationsBot conversation, CancellationToken ct = default)
        {
            await _ctx.ConversationsBots.AddAsync(conversation, ct);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task<ConversationsBot?> GetByIdAsync(int conversationId, CancellationToken ct = default)
            => await _ctx.ConversationsBots
                         .Include(c => c.MessagesBots)
                         .FirstOrDefaultAsync(c => c.ConversationId == conversationId, ct);

        public async Task<IEnumerable<ConversationsBot>> GetByUserAsync(int userId, CancellationToken ct = default)
            => await _ctx.ConversationsBots
                         .Where(c => c.UserId == userId)
                         .ToListAsync(ct);
    }
}
