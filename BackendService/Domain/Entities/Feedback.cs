using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int AccountId { get; set; }

    public int ProductId { get; set; }

    public string? Title { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? ImagePath { get; set; }

    public int? OrderDetailId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ReplyFeedback> ReplyFeedbacks { get; set; } = new List<ReplyFeedback>();
}
