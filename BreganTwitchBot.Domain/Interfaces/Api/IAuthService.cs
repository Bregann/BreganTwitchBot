using BreganTwitchBot.Domain.DTOs.Auth.Responses;

namespace BreganTwitchBot.Domain.Interfaces.Api
{
    public interface IAuthService
    {
        /// <summary>
        /// The Twitch url a visitor is sent to in order to sign in
        /// </summary>
        /// <param name="state">Anti forgery value echoed back on the callback</param>
        string BuildTwitchLoginUrl(string state);

        /// <summary>
        /// Exchanges the code Twitch handed back for our own session tokens,
        /// creating or updating the user as a side effect
        /// </summary>
        Task<LoginUserResponse> LoginWithTwitchAsync(string code);

        Task<LoginUserResponse> RefreshToken(string refreshToken);

        /// <summary>
        /// Revokes a refresh token so the session can't be resumed
        /// </summary>
        Task LogoutAsync(string refreshToken);

        /// <summary>
        /// Who the caller is, plus the channels they broadcast
        /// </summary>
        Task<CurrentUserResponse?> GetCurrentUserAsync(string twitchUserId);
    }
}
