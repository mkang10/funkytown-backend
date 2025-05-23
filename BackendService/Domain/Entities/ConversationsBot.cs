using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ConversationsBot
{
    public int ConversationId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Title { get; set; }

    public virtual ICollection<MessagesBot> MessagesBots { get; set; } = new List<MessagesBot>();

    public virtual Account User { get; set; } = null!;
}
