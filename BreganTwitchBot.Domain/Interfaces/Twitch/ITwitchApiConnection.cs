using BreganTwitchBot.Domain.Enums;
using static BreganTwitchBot.Domain.Services.Twitch.TwitchApiConnection;

namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITwitchApiConnection
    {
        Task InitialiseConnectionsAsync();
        public void Connect(string channelName, int databaseChannelId, string twitchChannelClientId, string accessToken, string refreshToken, AccountType type, string broadcasterChannelId, string broadcasterChannelName);
        public TwitchAccount? GetTwitchApiClientFromChannelName(string channelName);
        public TwitchAccount? GetBotTwitchApiClientFromBroadcasterChannelId(string broadcasterChannelId);
        public TwitchAccount[] GetAllApiClients();
        public TwitchAccount[] GetAllBotApiClients();
        Task RefreshAllApiKeys();
        string[] GetAllBroadcasterChannelIds();
    }
}
