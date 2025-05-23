using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public string? ConversationName { get; set; }

    public bool IsGroup { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
