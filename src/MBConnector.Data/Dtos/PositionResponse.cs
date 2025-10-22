using System.Text.Json.Serialization;

namespace MBConnector.Data.Dtos
{
    public sealed class PositionResponse
    {
        [JsonPropertyName("avgPrice")]
        public decimal AvgPrice { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("instrument")]
        public string Instrument { get; set; } = string.Empty;

        [JsonPropertyName("qty")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; } = string.Empty;
    }
}