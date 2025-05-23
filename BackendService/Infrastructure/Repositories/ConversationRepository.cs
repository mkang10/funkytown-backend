using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories
{
    public class ConversationRepository : IMessageRepository
    {
        private readonly FtownContext _context;

        public ConversationRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Conversation> CreateConversation(Conversation user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<ConversationParticipant> CreateConversationParticipants(ConversationParticipant user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Message> CreateMessage(Message user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteConversation(Conversation user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteConversationParticipant(List<ConversationParticipant> user)
        {
            _context.ConversationParticipants.RemoveRange(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessage(Message user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Pagination<Conversation>> GetAllConversationByAccountId(int id, PaginationParameter paginationParameter)
        {
            // dem account tham gia cuoc tro chuyen
            var itemCount = await _context.ConversationParticipants
                                           .Where(cp => cp.AccountId == id)
                                           .CountAsync();

            // liet ke moi conversation boi accId
            var conversationIds = await _context.ConversationParticipants
                                                 .Where(cp => cp.AccountId == id)
                                                 .Select(cp => cp.ConversationId)
                                                 .ToListAsync();

            // lay conversation dua theo thoi gian tin nhan gui moi nhat trong bang message
            var conversations = await _context.Conversations
                                               .Include(c => c.ConversationParticipants)
                                               .Include(c => c.Messages)
                                               .Where(c => conversationIds.Contains(c.ConversationId))
                                               .OrderByDescending
                                               (c => c.Messages.OrderByDescending(m => m.SentDate).FirstOrDefault().SentDate) // theo tin nhan moi nhat
                                               .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                               .Take(paginationParameter.PageSize)
                                               .AsNoTracking()
                                               .ToListAsync();

            var result = new Pagination<Conversation>(conversations, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<Pagination<Message>> GetAllMessageByConservationId(int id, PaginationParameter paginationParameter)
        {
            // counting message
            var itemCount = await _context.Messages
                                          .Where(m => m.ConversationId == id)
                                          .CountAsync();

            // get the latest 30 messages in conversation by conversation ID
            paginationParameter.PageSize = 30;
            var messages = await _context.Messages
                                          .Where(m => m.ConversationId == id)
                                          .OrderByDescending(m => m.SentDate) // Sắp xếp theo thời gian gửi mới nhất
                                          .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                          .Take(paginationParameter.PageSize)
                                          .AsNoTracking()
                                          .ToListAsync();

            var result = new Pagination<Message>(messages, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }


        public async Task<Conversation> GetConversationById(int id)
        {
            var data = await _context.Conversations.SingleOrDefaultAsync(x => x.ConversationId.Equals(id));
            return data;
        }

        public async Task<List<ConversationParticipant>> GetConversationParticipantById(int id)
        {
            var data = await _context.ConversationParticipants.Where(x => x.ConversationId == id).ToListAsync();
            return data;
        }

        public async Task<Message> GetMessageById(int id)
        {
            var data = await _context.Messages.SingleOrDefaultAsync(x => x.MessageId.Equals(id));
            return data;
        }


        // Account ========================
        public async Task<List<int>> GetAllShopManagerId()
        {
            // Retrieve all ShopManagerDetailIds
            var items = await _context.ShopManagerDetails
                                       .Select(c => c.ShopManagerDetailId)
                                       .ToListAsync();

            return items;
        }

        public async Task<List<int>> GetAllStaffId()
        {
            var items = await _context.StaffDetails
                                       .Select(c => c.StaffDetailId)
                                       .ToListAsync();

            return items;
        }
        public async Task<List<Message>> UpdateStatusIsReadRepository(List<Message> user)
        {
            _context.UpdateRange(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<List<Message>> GetMessagesByIdsAsync(List<int> ids)
        {
            return await _context.Messages
                .Where(m => ids.Contains(m.MessageId))
                .ToListAsync();
        }
    }
}
