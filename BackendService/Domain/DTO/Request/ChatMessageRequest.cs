using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class ChatMessageRequest
    {
        public int UserId { get; set; }
        public string Content { get; set; } = null!;
    }

    public class ChatCompletionChunk
    {
        public List<Choice>? Choices { get; set; }
        public class Choice { public Delta? Delta { get; set; } }
        public class Delta { public string? Content { get; set; } }
    }

    public class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "deepseek-chat";

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("functions")]
        public List<FunctionDefinition>? Functions { get; set; }

        [JsonPropertyName("function_call")]
        public object? FunctionCall { get; set; } = "auto";
    }
}
