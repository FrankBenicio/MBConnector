using System.Text.Json.Serialization;

namespace MBConnector.Data.Dtos
{
    public sealed class AuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonPropertyName("expiration")]
        public long Expiration { get; set; }
    }
}