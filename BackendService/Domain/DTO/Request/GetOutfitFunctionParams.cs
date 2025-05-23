using System.Text.Json.Serialization;

namespace Domain.DTO.Request
{
    /// <summary>
    /// Tham số để gợi ý phối đồ.
    /// </summary>
    public class GetOutfitFunctionParams
    {
        [JsonPropertyName("heightCm")]
        public int? HeightCm { get; set; }

        [JsonPropertyName("weightKg")]
        public int? WeightKg { get; set; }

        [JsonPropertyName("colorPreferences")]
        public List<string>? ColorPreferences { get; set; }

        [JsonPropertyName("occasion")]
        public string? Occasion { get; set; }

        [JsonPropertyName("style")]
        public string? Style { get; set; }
    }
}
