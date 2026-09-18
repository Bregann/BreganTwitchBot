using static BreganTwitchBot.Domain.Services.Twitch.TwitchApiConnection;

namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITwitchApiConnection
    {
        Task InitialiseConnectionsAsync();

        /// <summary>
        /// Connects the single bot account shared across every channel
        /// </summary>
        void ConnectBot();

        /// <summary>
        /// Connects a channel's own broadcaster account. Kept per channel as Twitch only exposes
        /// subscriptions, cheers, follows, polls and predictions to the broadcaster's own token.
        /// </summary>
        void ConnectBroadcaster(string channelName, int databaseChannelId, string broadcasterChannelId, string accessToken, string refreshToken);

        TwitchAccount? GetBotApiClient();
        TwitchAccount? GetBroadcasterApiClientFromChannelName(string channelName);
        TwitchAccount[] GetAllBroadcasterApiClients();
        string[] GetAllBroadcasterChannelIds();
        ChannelDetails[] GetAllChannels();
        ChannelDetails? GetChannelDetails(string broadcasterChannelId);
        Task RefreshAllApiKeys();
    }
}
