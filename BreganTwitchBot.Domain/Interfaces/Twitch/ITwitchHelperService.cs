namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITwitchHelperService
    {
        Task SendTwitchMessageToChannel(string broadcasterChannelId, string broadcasterChannelName, string message, string? originalMessageId = null);
        Task<string?> GetTwitchUserIdFromUsername(string username);
        Task<string?> GetPointsName(string broadcasterChannelName);
    }
}
