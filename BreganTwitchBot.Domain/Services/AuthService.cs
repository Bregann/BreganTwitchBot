using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Auth;
using BreganTwitchBot.Domain.DTOs.Auth.Responses;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Api;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BreganTwitchBot.Domain.Services
{
    /// <summary>
    /// Website authentication.
    ///
    /// Sign in is Twitch only. A viewer's Twitch id is the same id the bot stores on
    /// ChannelUser, so once someone has signed in their stats can be found without
    /// any account linking step.
    /// </summary>
    public class AuthService(AppDbContext context, IEnvironmentalSettingHelper environmentalSettingHelper, IHttpClientFactory httpClientFactory) : IAuthService
    {
        /// <summary>
        /// Twitch only needs to tell us who the user is, so no scopes are requested
        /// </summary>
        private const string TwitchScopes = "";

        public string BuildTwitchLoginUrl(string state)
        {
            var clientId = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPIClientID);
            var redirectUri = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.WebsiteTwitchOAuthRedirectUri);

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
            {
                throw new InvalidOperationException("The Twitch client id or website redirect uri is not configured");
            }

            return "https://id.twitch.tv/oauth2/authorize" +
                   $"?client_id={Uri.EscapeDataString(clientId)}" +
                   $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                   "&response_type=code" +
                   $"&scope={Uri.EscapeDataString(TwitchScopes)}" +
                   $"&state={Uri.EscapeDataString(state)}";
        }

        public async Task<LoginUserResponse> LoginWithTwitchAsync(string code)
        {
            var twitchUser = await ExchangeCodeForTwitchUserAsync(code);

            var user = await context.Users.FirstOrDefaultAsync(x => x.TwitchUserId == twitchUser.Id);

            if (user == null)
            {
                user = new User
                {
                    TwitchUserId = twitchUser.Id,
                    TwitchUsername = twitchUser.Login,
                    TwitchDisplayName = twitchUser.DisplayName,
                    ProfileImageUrl = twitchUser.ProfileImageUrl,
                    FirstLoggedInAt = DateTime.UtcNow,
                    LastLoggedInAt = DateTime.UtcNow
                };

                context.Users.Add(user);
                Log.Information($"[Auth] New website user {twitchUser.Login} ({twitchUser.Id})");
            }
            else
            {
                // keep the display details fresh, people rename themselves
                user.TwitchUsername = twitchUser.Login;
                user.TwitchDisplayName = twitchUser.DisplayName;
                user.ProfileImageUrl = twitchUser.ProfileImageUrl;
                user.LastLoggedInAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            await SaveRefreshToken(refreshToken, user.Id);

            return new LoginUserResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<LoginUserResponse> RefreshToken(string userRefreshToken)
        {
            var refreshToken = await context.UserRefreshTokens.FirstOrDefaultAsync(x => x.Token == userRefreshToken);

            if (refreshToken == null)
            {
                throw new KeyNotFoundException("Token not found");
            }

            if (refreshToken.IsRevoked)
            {
                // a revoked token being presented again can mean it was stolen, so the
                // whole family is dropped rather than just refusing this one
                await RevokeAllTokensForUser(refreshToken.UserId);
                Log.Warning($"[Auth] Revoked refresh token reused for user {refreshToken.UserId}, all sessions dropped");
                throw new UnauthorizedAccessException("Refresh token has been revoked");
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token expired");
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == refreshToken.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var accessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            refreshToken.IsRevoked = true;
            await SaveRefreshToken(newRefreshToken, user.Id);

            return new LoginUserResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var token = await context.UserRefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token == null)
            {
                return;
            }

            token.IsRevoked = true;
            await context.SaveChangesAsync();
        }

        public async Task<CurrentUserResponse?> GetCurrentUserAsync(string twitchUserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.TwitchUserId == twitchUserId);

            if (user == null)
            {
                return null;
            }

            var broadcasterOf = await context.Channels
                .Where(x => x.BroadcasterTwitchChannelId == twitchUserId)
                .Select(x => x.BroadcasterTwitchChannelName)
                .ToListAsync();

            return new CurrentUserResponse
            {
                TwitchUserId = user.TwitchUserId,
                TwitchUsername = user.TwitchUsername,
                TwitchDisplayName = user.TwitchDisplayName,
                ProfileImageUrl = user.ProfileImageUrl,
                BroadcasterOfChannels = broadcasterOf
            };
        }

        /// <summary>
        /// Swaps the one time code for a Twitch access token, then asks Twitch who it belongs to
        /// </summary>
        private async Task<TwitchUserInfo> ExchangeCodeForTwitchUserAsync(string code)
        {
            var clientId = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPIClientID);
            var clientSecret = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPISecret);
            var redirectUri = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.WebsiteTwitchOAuthRedirectUri);

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(redirectUri))
            {
                throw new InvalidOperationException("The Twitch oauth settings are not configured");
            }

            var httpClient = httpClientFactory.CreateClient();

            var tokenResponse = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri }
            }));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                Log.Error($"[Auth] Twitch token exchange failed with {tokenResponse.StatusCode}");
                throw new UnauthorizedAccessException("Could not sign in with Twitch");
            }

            var tokenBody = JsonConvert.DeserializeObject<TwitchOAuthTokenResponse>(await tokenResponse.Content.ReadAsStringAsync());

            if (tokenBody == null || string.IsNullOrWhiteSpace(tokenBody.AccessToken))
            {
                throw new UnauthorizedAccessException("Could not sign in with Twitch");
            }

            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/helix/users");
            userRequest.Headers.Add("Authorization", $"Bearer {tokenBody.AccessToken}");
            userRequest.Headers.Add("Client-Id", clientId);

            var userResponse = await httpClient.SendAsync(userRequest);

            if (!userResponse.IsSuccessStatusCode)
            {
                Log.Error($"[Auth] Twitch user lookup failed with {userResponse.StatusCode}");
                throw new UnauthorizedAccessException("Could not sign in with Twitch");
            }

            var users = JsonConvert.DeserializeObject<TwitchUsersEnvelope>(await userResponse.Content.ReadAsStringAsync());
            var twitchUser = users?.Data?.FirstOrDefault();

            if (twitchUser == null)
            {
                throw new UnauthorizedAccessException("Twitch did not return an account");
            }

            return twitchUser;
        }

        private static string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.TwitchUsername),
                new Claim("twitch_user_id", user.TwitchUserId),
                new Claim("twitch_username", user.TwitchUsername)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtKey")!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JwtValidIssuer"),
                audience: Environment.GetEnvironmentVariable("JwtValidAudience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(128));
        }

        private async Task SaveRefreshToken(string token, string userId)
        {
            context.UserRefreshTokens.Add(new UserRefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await context.SaveChangesAsync();
        }

        private async Task RevokeAllTokensForUser(string userId)
        {
            var tokens = await context.UserRefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await context.SaveChangesAsync();
        }

        private class TwitchUsersEnvelope
        {
            [JsonProperty("data")]
            public List<TwitchUserInfo>? Data { get; set; }
        }

        private class TwitchUserInfo
        {
            [JsonProperty("id")]
            public required string Id { get; set; }

            [JsonProperty("login")]
            public required string Login { get; set; }

            [JsonProperty("display_name")]
            public string? DisplayName { get; set; }

            [JsonProperty("profile_image_url")]
            public string? ProfileImageUrl { get; set; }
        }
    }
}
