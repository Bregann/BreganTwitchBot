using BreganTwitchBot.Domain.DTOs.Helpers;

namespace BreganTwitchBot.Domain.Interfaces.Helpers
{
    public interface IConfigHelperService
    {
        Task UpdateDailyPointsStatus(string broadcasterId, bool status);
        (bool DailyPointsAllowed, DateTime LastStreamDate, DateTime LastDailyPointedAllowedDate, bool StreamHappenedThisWeek) GetDailyPointsStatus(string broadcasterId);
        Task UpdateStreamLiveStatus(string broadcasterId, bool status);
        DiscordConfig? GetDiscordConfig(ulong discordGuildId);
        DiscordConfig? GetDiscordConfig(string broadcasterId);
        bool IsDiscordEnabled(string broadcasterId);

        /// <summary>
        /// The channel's currency name. Served from the config cache, so the Discord side can
        /// use it without going through the Twitch helper service.
        /// </summary>
        string GetPointsName(string broadcasterId);

        /// <summary>
        /// The currency name for the channel linked to a Discord guild
        /// </summary>
        string? GetPointsNameForGuild(ulong discordGuildId);
    }
}
