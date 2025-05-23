using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ReplyFeedback
{
    public int ReplyId { get; set; }

    public int FeedbackId { get; set; }

    public int AccountId { get; set; }

    public string ReplyText { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Feedback Feedback { get; set; } = null!;
}
