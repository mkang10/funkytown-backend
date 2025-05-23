using Domain.Commons;
using Domain.DTO.Request;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICommentService
    {
        public Task<Pagination<FeedbackRequestDTO>> GetAllFeedbackByProductId(int id, PaginationParameter paginationParameter);
        public Task<Pagination<FeedbackRequestDTO>> GettAllFeedbackByAccountId(int id, PaginationParameter paginationParameter);

        public Task<List<FeedbackRequestDTO>> CreateMultiple(List<CreateFeedBackArrayRequestDTO> feedbackRequests);
        public Task<FeedbackRequestDTO> Create(CreateFeedBackRequestDTO feedbackRequests);

        public Task<bool> Delete(int id);
        public Task<bool> Update(int id, UpdateFeedbackRequestDTO user);
        public Task<FeedbackRequestDTO> GetById(int id);

    }
}
