using Domain.Commons;
using Domain.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMessageService
    {
        public Task<Pagination<MessageRequest>> GetAllMessageServiceByConversationId(int id, PaginationParameter paginationParameter);
        public Task<MessageCreateRequest> createMessage(MessageCreateRequest user);
        public Task<List<MessageCreateRequest>> updateStatusIsRead(List<UpdateStatusIsReadMessageDTO> user);
        public Task<bool> deleteMessage(int id);
    }
}
