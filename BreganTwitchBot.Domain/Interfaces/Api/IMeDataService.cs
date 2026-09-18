using BreganTwitchBot.Domain.DTOs.Api;

namespace BreganTwitchBot.Domain.Interfaces.Api
{
    public interface IMeDataService
    {
        /// <summary>
        /// The caller's stats in every channel the bot has seen them in.
        /// Returns null when the bot has never seen them anywhere.
        /// </summary>
        Task<GetMyStatsResponse?> GetMyStatsAsync(string twitchUserId);

        /// <summary>
        /// The caller's own preferences, per channel
        /// </summary>
        Task<GetMySettingsResponse> GetMySettingsAsync(string twitchUserId);

        /// <summary>
        /// Updates the caller's preferences in one channel
        /// </summary>
        Task UpdateMySettingAsync(string twitchUserId, UpdateMySettingRequest request);
    }
}
