using BreganTwitchBot.Domain.Data.Services.Twitch.Commands;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.Extensions.Hosting;
using Serilog;
using TwitchLib.Api.Core.Enums;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    public class WebsocketHostedService(ITwitchApiConnection twitchApiConnection, CommandHandler commandHandler, ITwitchHelperService twitchHelperService) : IHostedService
    {
        private readonly Dictionary<string, EventSubWebsocketClient> _userConnections = [];

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
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Notification.Payload.Event.ChatterUserId,
                ChatterChannelName = args.Notification.Payload.Event.ChatterUserName,
                Message = args.Notification.Payload.Event.Message.Text,
                MessageParts = args.Notification.Payload.Event.Message.Text.Split(' '),
                MessageId = args.Notification.Payload.Event.MessageId,
                IsMod = args.Notification.Payload.Event.IsModerator,
                IsSub = args.Notification.Payload.Event.IsSubscriber,
                IsVip = args.Notification.Payload.Event.IsVip,
                IsBroadcaster = args.Notification.Payload.Event.IsBroadcaster
            };

            if (msgParams.Message.StartsWith('!'))
            {
                await commandHandler.HandleCommandAsync(msgParams.Message.Split(' ')[0], msgParams);
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
            //TODO: actually make this work
            //while (!await _eventSubWebsocketClient.ReconnectAsync())
            //{
            //    await Task.Delay(1000);
            //}
        }

        private async Task OnWebsocketConnected(object sender, WebsocketConnectedArgs e, string twitchChannelName)
        {
            if (!e.IsRequestedReconnect)
            {

                // Subscribe to events based on if a bot or a user
                var apiClient = twitchApiConnection.GetTwitchApiClientFromChannelName(twitchChannelName);
                var userWebsocketConnection = _userConnections.GetValueOrDefault(twitchChannelName);

                if (apiClient == null || userWebsocketConnection == null)
                {
                    return;
                }

                if (apiClient.Type == AccountType.Bot)
                {
                    // sub to bot specifc events, we get minimal permissions from the broadcaster and most from the bot
                    var conditions = new Dictionary<string, string>()
                        {
                            { "broadcaster_user_id", apiClient.BroadcasterChannelId },
                            { "user_id", apiClient.TwitchChannelClientId }
                        };

                    try
                    {
                        // TODO: migrate to this when supported - channel.moderate
                        // await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.moderate", "2", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.ActiveChannelId }, { "moderator_user_id", apiClient.TwitchChannelClientId } }, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId);

                        // TODO: add unban requests when my PR is merged in
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.chat.message", "1", conditions, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.update", "2", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.raid", "1", new Dictionary<string, string>() { { "to_broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("stream.online", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("stream.offline", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);

                        Log.Information($"[Twitch API Connection] Subscribed to bot events for {apiClient.TwitchUsername}");

                        await twitchHelperService.SendTwitchMessageToChannel(apiClient.BroadcasterChannelId, apiClient.BroadcasterChannelName, "hello currys (successfully connected)", null);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error subscribing to events for {apiClient.TwitchUsername}");
                    }
                }
                else
                {
                    // sub to broadcaster specific events
                    try
                    {
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.follow", "2", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId }, { "moderator_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscribe", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscription.gift", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscription.message", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.cheer", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.ban", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.unban", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.channel_points_automatic_reward_redemption.add", "2", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.channel_points_custom_reward_redemption.add", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.poll.begin", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.poll.end", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.begin", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.lock", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.end", "1", new Dictionary<string, string> { { "broadcaster_user_id", apiClient.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);

                        Log.Information($"[Twitch API Connection] Subscribed to broadcaster events for {apiClient.TwitchUsername}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error subscribing to events for {apiClient.TwitchUsername}");
                    }
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var apiClients = twitchApiConnection.GetAllApiClients();

            foreach (var apiClient in apiClients)
            {
                if (!_userConnections.ContainsKey(apiClient.TwitchUsername))
                {
                    var userWebSocket = new EventSubWebsocketClient();
                    _userConnections.Add(apiClient.TwitchUsername, userWebSocket);

                    if (apiClient.Type == AccountType.Bot)
                    {
                        userWebSocket.ChannelChatMessage += OnChannelChatMessageReceived;
                        userWebSocket.ChannelUpdate += OnChannelUpdate;
                        userWebSocket.ChannelRaid += OnChannelRaid;
                        userWebSocket.StreamOnline += OnStreamOnline;
                        userWebSocket.StreamOffline += OnStreamOffline;
                    }
                    else
                    {
                        userWebSocket.ChannelBan += OnChannelBan;
                        userWebSocket.ChannelUnban += OnChannelUnban;
                        userWebSocket.ChannelPointsAutomaticRewardRedemptionAdd += OnAutomaticRewardRedeemed;
                        userWebSocket.ChannelPointsCustomRewardAdd += OnCustomRewardRedeemed;
                        userWebSocket.ChannelPollBegin += OnPollBegin;
                        userWebSocket.ChannelPollEnd += OnPollEnd;
                        userWebSocket.ChannelPredictionBegin += OnChannelPredictionBegin;
                        userWebSocket.ChannelPredictionLock += OnChannelPredictionLock;
                        userWebSocket.ChannelPredictionEnd += OnChannelPredictionEnd;
                        userWebSocket.ChannelFollow += OnFollowReceived;
                        userWebSocket.ChannelSubscribe += OnChannelSubcribe;
                        userWebSocket.ChannelSubscriptionGift += OnChannelSubscriptionGift;
                        userWebSocket.ChannelSubscriptionMessage += OnChannelResubscribe;
                        userWebSocket.ChannelCheer += OnChannelCheer;
                    }

                    userWebSocket.WebsocketConnected += (sender, e) => OnWebsocketConnected(sender, e, apiClient.TwitchUsername);
                    userWebSocket.WebsocketDisconnected += OnWebsocketDisconnected;
                    userWebSocket.WebsocketReconnected += OnWebsocketReconnected;
                    userWebSocket.ErrorOccurred += OnErrorOccurred;

                    await userWebSocket.ConnectAsync();
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var user in _userConnections)
            {
                await user.Value.DisconnectAsync();
            }
        }
    }
}
