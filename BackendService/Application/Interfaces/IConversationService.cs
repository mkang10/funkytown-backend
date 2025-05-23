using Domain.Commons;
using Domain.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IConversationService
    {
        public Task<Pagination<ConversationRequest>> GetAllConversationServiceByAccountId(int id,PaginationParameter paginationParameter);
        public Task<ConversationCreateRequest> createConversation(ConversationCreateRequest user);
        public Task<bool> deleteConversation(int id);

    }
}
