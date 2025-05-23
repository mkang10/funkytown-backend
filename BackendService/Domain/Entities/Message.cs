using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Message
{
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public int SenderId { get; set; }

    public string MessageContent { get; set; } = null!;

    public DateTime SentDate { get; set; }

    public int? ParentMessageId { get; set; }

    public bool IsRead { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual ICollection<Message> InverseParentMessage { get; set; } = new List<Message>();

    public virtual Message? ParentMessage { get; set; }

    public virtual Account Sender { get; set; } = null!;
}
