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

            // Events subcribed to via bot account
            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessageReceived;
            _eventSubWebsocketClient.ChannelUpdate += OnChannelUpdate;
            _eventSubWebsocketClient.ChannelRaid += OnChannelRaid;
            _eventSubWebsocketClient.ChannelBan += OnChannelBan;
            _eventSubWebsocketClient.ChannelUnban += OnChannelUnban;
            _eventSubWebsocketClient.StreamOnline += OnStreamOnline;
            _eventSubWebsocketClient.StreamOffline += OnStreamOffline;

            // Events subcribed to via broadcaster account
            _eventSubWebsocketClient.ChannelPointsAutomaticRewardRedemptionAdd += OnAutomaticRewardRedeemed;
            _eventSubWebsocketClient.ChannelPointsCustomRewardAdd += OnCustomRewardRedeemed;
            _eventSubWebsocketClient.ChannelPollBegin += OnPollBegin;
            _eventSubWebsocketClient.ChannelPollEnd += OnPollEnd;
            _eventSubWebsocketClient.ChannelPredictionBegin += OnChannelPredictionBegin;
            _eventSubWebsocketClient.ChannelPredictionLock += OnChannelPredictionLock;
            _eventSubWebsocketClient.ChannelPredictionEnd += OnChannelPredictionEnd;

            _eventSubWebsocketClient.ChannelFollow += OnFollowReceived;
            _eventSubWebsocketClient.ChannelSubscribe += OnChannelSubcribe;
            _eventSubWebsocketClient.ChannelSubscriptionGift += OnChannelSubscriptionGift;
            _eventSubWebsocketClient.ChannelSubscriptionMessage += OnChannelResubscribe;
            _eventSubWebsocketClient.ChannelCheer += OnChannelCheer;

            _twitchApiConnection = twitchApiConnection;
            _commandHandler = commandHandler;
        }

        private Task OnChannelCheer(object sender, ChannelCheerArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelResubscribe(object sender, ChannelSubscriptionMessageArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelSubscriptionGift(object sender, ChannelSubscriptionGiftArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelSubcribe(object sender, ChannelSubscribeArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnFollowReceived(object sender, ChannelFollowArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnStreamOffline(object sender, TwitchLib.EventSub.Websockets.Core.EventArgs.Stream.StreamOfflineArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnStreamOnline(object sender, TwitchLib.EventSub.Websockets.Core.EventArgs.Stream.StreamOnlineArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelUnban(object sender, ChannelUnbanArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelPredictionEnd(object sender, ChannelPredictionEndArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelPredictionLock(object sender, ChannelPredictionLockArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelPredictionBegin(object sender, ChannelPredictionBeginArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnPollEnd(object sender, ChannelPollEndArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnPollBegin(object sender, ChannelPollBeginArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnCustomRewardRedeemed(object sender, ChannelPointsCustomRewardArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnAutomaticRewardRedeemed(object sender, ChannelPointsAutomaticRewardRedemptionArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelBan(object sender, ChannelBanArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelRaid(object sender, ChannelRaidArgs args)
        {
            throw new NotImplementedException();
        }

        private Task OnChannelUpdate(object sender, ChannelUpdateArgs args)
        {
            throw new NotImplementedException();
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
                            //TODO: migrate to this when supported - channel.moderate
                            // await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.moderate", "2", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.ActiveChannelId }, { "moderator_user_id", apiClient.TwitchChannelClientId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);

                            // TODO: add unban requests when my PR is merged in
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.chat.message", "1", conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.update", "2", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.raid", "1", new Dictionary<string, string>() { { "to_broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("stream.online", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("stream.offline", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error subscribing to events for {apiClient.TwitchUsername}");
                            continue;
                        }
                        
                        Log.Information($"[Twitch API Connection] Subscribed to events for {apiClient.ActiveChannelId}");
                    }
                    else
                    {
                        // sub to broadcaster specific events
                        try
                        {
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.follow", "2", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId }, { "moderator_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscribe", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscription.gift", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscription.message", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.cheer", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.ban", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.unban", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.channel_points_automatic_reward_redemption.add", "2", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.channel_points_custom_reward_redemption.add", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.poll.begin", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.poll.end", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.begin", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.lock", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);
                            await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.end", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.ActiveChannelId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);

                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error subscribing to events for {apiClient.TwitchUsername}");
                            continue;
                        }
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
