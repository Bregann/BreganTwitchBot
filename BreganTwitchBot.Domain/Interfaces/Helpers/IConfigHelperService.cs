using BreganTwitchBot.Domain.DTOs.Helpers;

namespace BreganTwitchBot.Domain.Interfaces.Helpers
{
    public interface IConfigHelperService
    {
        Task UpdateDailyPointsStatus(string broadcasterId, bool status);
        (bool DailyPointsAllowed, DateTime LastStreamDate, DateTime LastDailyPointedAllowedDate, bool StreamHappenedThisWeek) GetDailyPointsStatus(string broadcasterId);
        DateTime GetLastStreamEndDate(string broadcasterId);

        /// <summary>
        /// Sets the stream live/offline. <paramref name="streamStartedAt"/> is the real start time when
        /// picking up a stream that was already live (e.g. after a reboot), otherwise now is used
        /// </summary>
        Task UpdateStreamLiveStatus(string broadcasterId, bool status, DateTime? streamStartedAt = null);
        DiscordConfig? GetDiscordConfig(ulong discordGuildId);
        DiscordConfig? GetDiscordConfig(string broadcasterId);
        bool IsDiscordEnabled(string broadcasterId);
        int GetAutoShoutoutMinimumViewers(string broadcasterId);
        string GetPointsName(string broadcasterId);
        string? GetPointsNameForGuild(ulong discordGuildId);
    }
}
