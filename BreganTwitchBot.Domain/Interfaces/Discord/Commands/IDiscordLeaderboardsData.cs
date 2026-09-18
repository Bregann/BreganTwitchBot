using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordLeaderboardsData
    {
        /// <summary>
        /// The top entries of a leaderboard for the channel linked to this guild
        /// </summary>
        /// <param name="take">How many places to return. Discord embeds allow 25 fields.</param>
        Task<List<LeaderboardEntry>> GetLeaderboardAsync(ulong guildId, DiscordLeaderboardType type, int take = 24);
    }
}
