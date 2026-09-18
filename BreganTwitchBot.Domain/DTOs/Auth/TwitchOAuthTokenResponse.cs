using Newtonsoft.Json;

namespace BreganTwitchBot.Domain.DTOs.Auth
{
    public class TwitchOAuthTokenResponse
    {
        [JsonProperty("access_token")]
        public required string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
