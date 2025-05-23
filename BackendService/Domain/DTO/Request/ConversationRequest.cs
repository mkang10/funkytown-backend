using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ConversationRequest
    {
        public int ConversationId { get; set; }

        public string? ConversationName { get; set; }

        public bool IsGroup { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class ConversationCreateRequest
    {
        public string? ConversationName { get; set; }

        public bool IsGroup { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<int> ParticipantIds { get; set; } = new List<int>();

    }


}

