using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ChatBot
{
    public int ChatBotId { get; set; }

    public string? Key { get; set; }

    public string? BaseUrl { get; set; }

    public string? Context { get; set; }

    public bool IsDefault { get; set; }
}
