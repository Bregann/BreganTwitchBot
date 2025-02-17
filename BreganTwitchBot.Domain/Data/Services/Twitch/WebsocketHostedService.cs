using BreganTwitchBot.Domain.Data.Services.Twitch.Commands;
using BreganTwitchBot.Domain.Enums;
using Microsoft.Extensions.Hosting;
using Serilog;
using TwitchLib.Api.Core.Enums;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    public class WebsocketHostedService : IHostedService
    {
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;
        private readonly TwitchApiConnection _twitchApiConnection;
        private readonly CommandHandler _commandHandler;

        public WebsocketHostedService(EventSubWebsocketClient eventSubWebsocketClient, TwitchApiConnection twitchApiConnection, CommandHandler commandHandler)
        {
            _eventSubWebsocketClient = eventSubWebsocketClient ?? throw new ArgumentNullException(nameof(eventSubWebsocketClient));
            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessageReceived;

            _twitchApiConnection = twitchApiConnection;
            _commandHandler = commandHandler;
        }

        private async Task OnChannelChatMessageReceived(object sender, ChannelChatMessageArgs args)
        {
            var messageContent = args.Notification.Payload.Event.Message.Text;

            if (messageContent.StartsWith('!'))
            {
                await _commandHandler.HandleCommandAsync(messageContent.Split(' ')[0], args);
            }
        }

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            Log.Fatal(args.Exception, "Websocket error occurred");
            return Task.CompletedTask;
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            Log.Information("Websocket reconnected");
            return Task.CompletedTask;
        }

        private async Task OnWebsocketDisconnected(object sender, EventArgs e)
        {

            // Don't do this in production. You should implement a better reconnect strategy with exponential backoff
            while (!await _eventSubWebsocketClient.ReconnectAsync())
            {
                await Task.Delay(1000);
            }
        }

        private async Task OnWebsocketConnected(object sender, WebsocketConnectedArgs e)
        {
            if (!e.IsRequestedReconnect)
            {
                // Subscribe to events based on if a bot or a user
                var apiClients = _twitchApiConnection.GetAllApiClients();

                foreach (var apiClient in apiClients)
                {
                    var scopes = apiClient.ApiClient.Settings.Scopes;


                    if (apiClient.Type == AccountType.Bot)
                    {
                        // sub to bot specifc events, we get minimal permissions from the broadcaster and most from the bot
                        var conditions = new Dictionary<string, string>()
                        {
                            { "broadcaster_user_id", apiClient.ActiveChannelId },
                            { "user_id", apiClient.TwitchChannelClientId }
                        };

                        try
                        {
                            var subscriptionResponse = await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.chat.message", "1", conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error subscribing to channel.chat.message for {apiClient.ActiveChannelId}");
                        }
                        
                        Log.Information($"[Twitch API Connection] Subscribed to channel.chat.message for {apiClient.ActiveChannelId}");
                    }
                    else
                    {
                        // sub to broadcaster specific events
                    }
                }

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
