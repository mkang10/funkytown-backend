using Domain.Commons;
using Domain.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReplyFeedbackService
    {
        public Task<Pagination<ReplyRequestDTO>> GettAllReplyByFeedbackId(int id, PaginationParameter paginationParameter);
        public Task<CreateReplyRequestDTO> Create(CreateReplyRequestDTO user);
        public Task<bool> Delete(int id);
        public Task<bool> Update(int id, UpdateReplyRequestDTO user);
        public Task<ReplyRequestDTO> GetById(int id);
    }
}
