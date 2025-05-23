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
    // Infrastructure/Repositories/ChatBotRepository.cs
    public class ChatBotRepository : IChatBotRepository
    {
        private readonly FtownContext _ctx;
        public ChatBotRepository(FtownContext ctx) => _ctx = ctx;

        public async Task<ChatBot?> GetDefaultAsync(CancellationToken ct = default)
        {
            // Lấy bot có IsDefault = true
            var bot = await _ctx.ChatBots
                                .FirstOrDefaultAsync(b => b.IsDefault == true, ct);
            if (bot != null)
                return bot;

            // Fallback: lấy bản ghi đầu tiên theo ID
            return await _ctx.ChatBots
                             .OrderBy(b => b.ChatBotId)
                             .FirstOrDefaultAsync(ct);
        }

    }

}
