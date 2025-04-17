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
    }
}
