using Domain.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICommentRepository
    {
        public Task<Pagination<Feedback>> GettAllFeedbackByProductId(int id, PaginationParameter paginationParameter);
        public Task<Pagination<Feedback>> GettAllCommentByAccountId(int id, PaginationParameter paginationParameter);

        public Task<Feedback> CreateFeedback(Feedback data);
        public Task<Feedback> UpdateFeedback(Feedback data);
        public Task<bool> DeleteFeedback(Feedback data);
        //===================================================================================================================
        public Task<Pagination<ReplyFeedback>> GettAllReplyByFeedbackId(int id, PaginationParameter paginationParameter);
        public Task<ReplyFeedback> CreateReply(ReplyFeedback data);
        public Task<ReplyFeedback> UpdateReply(ReplyFeedback data);
        public Task<bool> DeleteReply(ReplyFeedback data);
        //===================================================================================================================

        public Task<Feedback> GetFeedBackById(int id);
        public Task<ReplyFeedback> GetReplyFeedBackById(int id);
        public Task<Order> UpdateStatusIsFeedback(Order data);


    }
}
