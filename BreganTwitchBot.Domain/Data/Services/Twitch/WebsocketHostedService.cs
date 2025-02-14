using Microsoft.Extensions.Hosting;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    public class WebsocketHostedService : IHostedService
    {
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;
        private readonly TwitchApiConnection _twitchApiConnection;

        public WebsocketHostedService(EventSubWebsocketClient eventSubWebsocketClient, TwitchApiConnection twitchApiConnection)
        {
            _eventSubWebsocketClient = eventSubWebsocketClient ?? throw new ArgumentNullException(nameof(eventSubWebsocketClient));
            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.ChannelFollow += OnChannelFollow;
            _twitchApiConnection = twitchApiConnection;
        }

        private Task OnErrorOccurred(object sender, TwitchLib.EventSub.Websockets.Core.EventArgs.ErrorOccuredArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            throw new NotImplementedException();
        }

        private async Task OnWebsocketDisconnected(object sender, EventArgs e)
        {

            // Don't do this in production. You should implement a better reconnect strategy with exponential backoff
            while (!await _eventSubWebsocketClient.ReconnectAsync())
            {
                await Task.Delay(1000);
            }
        }

        private Task OnWebsocketConnected(object sender, WebsocketConnectedArgs e)
        {
            if (!e.IsRequestedReconnect)
            {
                /*
                Subscribe to topics via the TwitchApi.Helix.EventSub object, this example shows how to subscribe to the channel follow event used in the example above.

                var conditions = new Dictionary<string, string>()
                {
                    { "broadcaster_user_id", someUserId }
                };
                var subscriptionResponse = await TwitchApi.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.follow", "2", conditions,
                EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);

                You can find more examples on the subscription types and their requirements here https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/
                Prerequisite: Twitchlib.Api nuget package installed (included in the Twitchlib package automatically)
                */
            }
            return Task.CompletedTask;

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.DisconnectAsync();
        }
    }
}
