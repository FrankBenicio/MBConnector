using System.Text.Json.Serialization;

namespace MBConnector.Data.Dtos
{
    public sealed class AccountResponse
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("currencySign")]
        public string CurrencySign { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}