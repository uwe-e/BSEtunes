using System.Text.Json.Serialization;

namespace BSEtunes.Identity.DTOs
{
    public class TokenResponseDto
    {
        [JsonPropertyName("expires_in")]
        public int Expires { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = null!;
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = null!;
        
    }
}
