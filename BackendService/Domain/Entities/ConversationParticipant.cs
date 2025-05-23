using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ConversationParticipant
{
    public int ConversationId { get; set; }

    public int AccountId { get; set; }

    public DateTime JoinedDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Conversation Conversation { get; set; } = null!;
}
