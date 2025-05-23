using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class FeedbackRequestDTO
    {
        public int FeedbackId { get; set; }

        public string Account { get; set; }

        public string Product { get; set; }

        public string? Title { get; set; }

        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ImagePath { get; set; }

        public int? OrderDetailId { get; set; }
    }

    public class ReplyRequestDTO
    {
        public int ReplyId { get; set; }

        public int FeedbackId { get; set; }

        public string Account { get; set; }

        public string ReplyText { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }
    }
    public class UpdateOrderStatusDTO
    {
        public bool? IsFeedback { get; set; }

    }
}
