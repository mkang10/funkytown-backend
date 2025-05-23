using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class MessagesBot
{
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public string Sender { get; set; } = null!;

    public string MessageContent { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public virtual ConversationsBot Conversation { get; set; } = null!;
}
