using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Api
{
    public interface IApiDataService
    {
        /// <summary>
        /// A leaderboard for a channel. Returns null when the channel isn't known.
        /// </summary>
        Task<GetLeaderboardResponse?> GetLeaderboardAsync(string broadcasterChannelName, DiscordLeaderboardType type, int take = 250);

        /// <summary>
        /// The custom commands configured in a channel
        /// </summary>
        Task<List<GetCustomCommandResponse>?> GetCustomCommandsAsync(string broadcasterChannelName);

        /// <summary>
        /// The state of a channel's subathon
        /// </summary>
        Task<GetSubathonStatusResponse?> GetSubathonStatusAsync(string broadcasterChannelName);

        /// <summary>
        /// Who has contributed most to a channel's subathon
        /// </summary>
        Task<GetSubathonLeaderboardResponse?> GetSubathonLeaderboardAsync(string broadcasterChannelName, int take = 10);

        /// <summary>
        /// A channel's headline numbers, for the channel home page
        /// </summary>
        Task<GetChannelSummaryResponse?> GetChannelSummaryAsync(string broadcasterChannelName);

        /// <summary>
        /// Past streams for a channel, most recent first
        /// </summary>
        Task<List<GetStreamHistoryResponse>?> GetStreamHistoryAsync(string broadcasterChannelName, int take = 30);
    }
}
