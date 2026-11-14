using BreganTwitchBot.Domain.DTOs.Helpers;

namespace BreganTwitchBot.Domain.Interfaces.Helpers
{
    public interface IConfigHelperService
    {
        Task UpdateDailyPointsStatus(string broadcasterId, bool status);
        (bool DailyPointsAllowed, DateTime LastStreamDate, DateTime LastDailyPointedAllowedDate, bool StreamHappenedThisWeek) GetDailyPointsStatus(string broadcasterId);
        Task UpdateStreamLiveStatus(string broadcasterId, bool status);

        /// <summary>
        /// Marks the stream live on this twitch broadcast. The stream start is only moved on for a
        /// new broadcast, so one that drops and comes back keeps its original start.
        /// </summary>
        Task MarkStreamLive(string broadcasterId, string twitchStreamId, DateTime? newBroadcastStartedAt);

        StreamState GetStreamState(string broadcasterId);
        DiscordConfig? GetDiscordConfig(ulong discordGuildId);
        DiscordConfig? GetDiscordConfig(string broadcasterId);
        bool IsDiscordEnabled(string broadcasterId);
        int GetAutoShoutoutMinimumViewers(string broadcasterId);
        string GetPointsName(string broadcasterId);
        string? GetPointsNameForGuild(ulong discordGuildId);
    }
}
