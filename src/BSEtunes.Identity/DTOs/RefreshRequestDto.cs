using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Identity.DTOs
{
    public record RefreshRequestDto(
        [property: JsonPropertyName("user_id")]
        string UserId,
        
        [property: JsonPropertyName("refresh_token")]
        string RefreshToken);
}
