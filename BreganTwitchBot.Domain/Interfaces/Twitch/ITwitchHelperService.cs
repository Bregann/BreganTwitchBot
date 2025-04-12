namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITwitchHelperService
    {
        Task SendTwitchMessageToChannel(string broadcasterChannelId, string broadcasterChannelName, string message, string? originalMessageId = null);
        Task<string?> GetTwitchUserIdFromUsername(string username);
        Task<string> GetPointsName(string broadcasterChannelId, string broadcasterChannelName = "");
        Task<bool> IsUserSuperModInChannel(string broadcasterChannelId, string viewerChannelId);
        Task EnsureUserHasModeratorPermissions(bool isMod, bool isBroadcaster, string viewerUsername, string viewerChannelId, string broadcasterChannelId, string broadcasterChannelName);
        Task AddPointsToUser(string broadcasterChannelId, string viewerChannelId, long pointsToAdd, string broadcasterChannelName, string viewerUsername);
        Task<bool> IsBroadcasterLive(string broadcasterChannelId);
        Task<long> GetPointsForUser(string broadcasterChannelId, string userChannelId, string broadcasterUsername, string userChannelName);
        Task RemovePointsFromUser(string broadcasterChannelId, string viewerChannelId, long pointsToRemove, string broadcasterChannelName, string viewerUsername);
        Task AddOrUpdateUserToDatabase(string broadcasterChannelId, string userChannelId, string broadcasterUsername, string userChannelName, bool addMinutes = false);
        Task SendAnnouncementMessageToChannel(string broadcasterChannelId, string broadcasterChannelName, string message);
        Task WarnUser(string broadcasterChannelId, string userId, string message);
        Task TimeoutUser(string broadcasterChannelId, string userId, int timeoutDurationInSeconds, string reason);
        Task BanUser(string broadcasterChannelId, string userId, string reason);
    }
}
