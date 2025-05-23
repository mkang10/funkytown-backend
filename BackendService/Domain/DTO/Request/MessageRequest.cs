using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class MessageRequest
    {
        public int MessageId { get; set; }

        public int ConversationId { get; set; }

        public int SenderId { get; set; }

        public string MessageContent { get; set; } = null!;

        public DateTime SentDate { get; set; }

        public int? ParentMessageId { get; set; }

        public bool IsRead { get; set; }
    }

    public class MessageCreateRequest
    {
        [JsonIgnore]
        public int MessageId { get; set; }

        public int ConversationId { get; set; }

        public int SenderId { get; set; }

        public string MessageContent { get; set; } = null!;

        public DateTime SentDate { get; set; }

        public int? ParentMessageId { get; set; }

        public bool IsRead { get; set; }
    }

    public class UpdateStatusIsReadMessageDTO
    {
        public int id { get; set; }
        public bool IsRead = true;
    }
}

