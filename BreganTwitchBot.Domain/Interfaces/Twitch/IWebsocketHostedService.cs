namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface IWebsocketHostedService
    {
        /// <summary>
        /// Recreates any EventSub subscriptions Twitch has lost and reconnects any websocket that
        /// has died without us noticing
        /// </summary>
        Task VerifyConnectionsAsync();
    }
}
