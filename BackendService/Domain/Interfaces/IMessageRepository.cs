using Domain.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IMessageRepository
    {
        public Task<Pagination<Message>> GetAllMessageByConservationId(int id, PaginationParameter paginationParameter);
        public Task<Message> CreateMessage(Message user);
        public Task<bool> DeleteMessage(Message user);
        public Task<List<Message>> UpdateStatusIsReadRepository(List<Message> user);


        public Task<Pagination<Conversation>> GetAllConversationByAccountId(int id, PaginationParameter paginationParameter);
        public Task<Conversation> CreateConversation(Conversation user);
        public Task<bool> DeleteConversation(Conversation user);

        public Task<Conversation> GetConversationById(int id);
        public Task<Message> GetMessageById(int id);

        public Task<List<ConversationParticipant>> GetConversationParticipantById(int id);


        public Task<ConversationParticipant> CreateConversationParticipants(ConversationParticipant user);
        public Task<bool> DeleteConversationParticipant(List<ConversationParticipant> user);

        public Task<List<int>> GetAllShopManagerId();
        public Task<List<int>> GetAllStaffId();
        public Task<List<Message>> GetMessagesByIdsAsync(List<int> ids);

    }
}
