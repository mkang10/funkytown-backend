// Domain/Models/FunctionDefinition.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class FunctionDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("parameters")]
        public JsonElement Parameters { get; set; }
    }
}
