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
    }
}
