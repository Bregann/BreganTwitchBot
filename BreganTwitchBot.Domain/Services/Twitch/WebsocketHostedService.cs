using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch.Events;
using Microsoft.Extensions.Hosting;
using Serilog;
using TwitchLib.Api.Core.Enums;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Core.EventArgs.Stream;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace BreganTwitchBot.Domain.Services.Twitch
{
    public class WebsocketHostedService(
        ITwitchApiConnection twitchApiConnection,
        ICommandHandler commandHandler,
        ITwitchHelperService twitchHelperService,
        ITwitchEventHandlerService twitchEventHandlerService,
        IConfigHelperService configHelperService,
        IWordBlacklistMonitorService wordBlacklistMonitorService,
        ITwitchApiInteractionService twitchApiInteractionService
    ) : IHostedService
    {
        private readonly Dictionary<string, EventSubWebsocketClient> _userConnections = [];

        private async Task OnChannelCheer(object sender, ChannelCheerArgs args)
        {
            var bitsEvent = new BitsCheeredParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Payload.Event.UserId ?? "Anon",
                ChatterChannelName = args.Payload.Event.UserName ?? "Anon",
                Amount = args.Payload.Event.Bits,
                Message = args.Payload.Event.Message,
                IsAnonymous = args.Payload.Event.IsAnonymous
            };

            Log.Information($"[Twitch Events] Bits cheered: {bitsEvent.Amount} from {bitsEvent.ChatterChannelName} in {bitsEvent.BroadcasterChannelName}");

            await twitchEventHandlerService.HandleChannelCheerEvent(bitsEvent);
        }

        private async Task OnChannelResubscribe(object sender, ChannelSubscriptionMessageArgs args)
        {
            var resubEvent = new ChannelResubscribeParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Payload.Event.UserId,
                ChatterChannelName = args.Payload.Event.UserName,
                Message = args.Payload.Event.Message.Text,
                StreakMonths = args.Payload.Event.StreakMonths,
                CumulativeMonths = args.Payload.Event.CumulativeMonths,
                SubTier = Enum.TryParse<SubTierEnum>(args.Payload.Event.Tier, out var tier) ? tier : SubTierEnum.Tier1
            };

            Log.Information($"[Twitch Events] Channel resubscribe: {resubEvent.ChatterChannelName} in {resubEvent.BroadcasterChannelName}. Tier: {resubEvent.SubTier}");

            await twitchEventHandlerService.HandleChannelResubscribeEvent(resubEvent);
        }

        private async Task OnChannelSubscriptionGift(object sender, ChannelSubscriptionGiftArgs args)
        {
            var giftSubEvent = new ChannelGiftSubParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Payload.Event.UserId,
                ChatterChannelName = args.Payload.Event.UserName,
                IsAnonymous = args.Payload.Event.IsAnonymous,
                SubTier = Enum.TryParse<SubTierEnum>(args.Payload.Event.Tier, out var tier) ? tier : SubTierEnum.Tier1,
                CumulativeTotal = args.Payload.Event.CumulativeTotal,
                Total = args.Payload.Event.Total
            };

            Log.Information($"[Twitch Events] Channel gift sub: {giftSubEvent.ChatterChannelName} in {giftSubEvent.BroadcasterChannelName}. Tier: {giftSubEvent.SubTier}");

            await twitchEventHandlerService.HandleChannelGiftSubEvent(giftSubEvent);
        }

        private async Task OnChannelSubcribe(object sender, ChannelSubscribeArgs args)
        {
            var subEvent = new ChannelSubscribeParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Payload.Event.UserId,
                ChatterChannelName = args.Payload.Event.UserName,
                SubTier = Enum.TryParse<SubTierEnum>(args.Payload.Event.Tier, out var tier) ? tier : SubTierEnum.Tier1,
                IsGift = args.Payload.Event.IsGift
            };

            Log.Information($"[Twitch Events] Channel subscribe: {subEvent.ChatterChannelName} in {subEvent.BroadcasterChannelName}. Tier: {subEvent.SubTier}");

            await twitchEventHandlerService.HandleChannelSubEvent(subEvent);
        }

        private async Task OnFollowReceived(object sender, ChannelFollowArgs args)
        {
            Log.Information($"[Twitch Events] Channel follow: {args.Payload.Event.UserName} ({args.Payload.Event.UserId}) in {args.Payload.Event.BroadcasterUserName}");
            await twitchHelperService.AddOrUpdateUserToDatabase(args.Payload.Event.BroadcasterUserId, args.Payload.Event.UserId, args.Payload.Event.BroadcasterUserName, args.Payload.Event.UserName);
        }

        private async Task OnStreamOffline(object sender, StreamOfflineArgs args)
        {
            Log.Information($"[Twitch Events] Stream offline: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId})");

            await configHelperService.UpdateStreamLiveStatus(args.Payload.Event.BroadcasterUserId, false);
            await configHelperService.UpdateDailyPointsStatus(args.Payload.Event.BroadcasterUserId, false);
        }

        private async Task OnStreamOnline(object sender, StreamOnlineArgs args)
        {
            Log.Information($"[Twitch Events] Stream online: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId})");

            await twitchEventHandlerService.HandleStreamOnline(args.Payload.Event.BroadcasterUserId, args.Payload.Event.BroadcasterUserName);
        }

        private async Task OnChannelBan(object sender, ChannelBanArgs args)
        {
            Log.Information($"[Twitch Events] Channel ban: {args.Payload.Event.UserName} ({args.Payload.Event.UserId}) in {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId})");
        }

        private async Task OnChannelUnban(object sender, ChannelUnbanArgs args)
        {
            Log.Information($"[Twitch Events] Channel unban: {args.Payload.Event.UserName} ({args.Payload.Event.UserId}) in {args.Payload.Event.BroadcasterUserName}");
        }

        private async Task OnChannelPredictionEnd(object sender, ChannelPredictionEndArgs args)
        {
            var predictionEndParams = new ChannelPredictionEndParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                PredictionId = args.Payload.Event.Id,
                WonOutcome = args.Payload.Event.Outcomes.Where(x => x.Id == args.Payload.Event.WinningOutcomeId).Select(x => new ChannelPredictionEndParams.Outcome
                {
                    Id = x.Id,
                    Title = x.Title,
                    Users = x.Users ?? 0,
                    ChannelPoints = x.ChannelPoints ?? 0,
                    TopPredictors = x.TopPredictors.Select(y => new ChannelPredictionEndParams.TopPredictor
                    {
                        UserName = y.UserName,
                        UserId = y.UserId,
                        ChannelPointsWon = y.ChannelPointsWon,
                        ChannelPointsUsed = y.ChannelPointsUsed
                    }).ToArray()
                }).First(),
                PredictionStatus = args.Payload.Event.Status,
                PredictionTitle = args.Payload.Event.Title
            };

            Log.Information($"[Twitch Events] Channel prediction end: {predictionEndParams.BroadcasterChannelName} ({predictionEndParams.BroadcasterChannelId}) - {predictionEndParams.PredictionTitle}");

            await twitchEventHandlerService.HandlePredictionEndEvent(predictionEndParams);
        }

        private async Task OnChannelPredictionLock(object sender, ChannelPredictionLockArgs args)
        {
            var predictionLockParams = new ChannelPredictionLockedParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                PredictionId = args.Payload.Event.Id,
                PredictionTitle = args.Payload.Event.Title,
                PredictionStatus = "Locked"
            };

            Log.Information($"[Twitch Events] Channel prediction lock: {predictionLockParams.BroadcasterChannelName} ({predictionLockParams.BroadcasterChannelId}) - {predictionLockParams.PredictionTitle}");

            await twitchEventHandlerService.HandlePredictionLockedEvent(predictionLockParams);
        }

        private async Task OnChannelPredictionBegin(object sender, ChannelPredictionBeginArgs args)
        {
            var predictionBeginParams = new ChannelPredictionBeginParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                PredictionId = args.Payload.Event.Id,
                PredictionTitle = args.Payload.Event.Title,
                PredictionStatus = "Started",
                PredictionOutcomeOptions = args.Payload.Event.Outcomes.Select(x => new PredictionOutcomeOption
                {
                    Id = x.Id,
                    Title = x.Title
                }).ToArray()
            };

            Log.Information($"[Twitch Events] Channel prediction begin: {predictionBeginParams.BroadcasterChannelName} ({predictionBeginParams.BroadcasterChannelId}) - {predictionBeginParams.PredictionTitle}");

            await twitchEventHandlerService.HandlePredictionBeginEvent(predictionBeginParams);
        }

        private async Task OnPollEnd(object sender, ChannelPollEndArgs args)
        {
            var pollEndParams = new ChannelPollEndParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                PollTitle = args.Payload.Event.Title,
                PollEndResults = args.Payload.Event.Choices.Select(x => new PollEndChoices
                {
                    Id = x.Id,
                    Title = x.Title,
                    Votes = x.Votes ?? 0,
                    ChannelPointsVotes = x.ChannelPointsVotes ?? 0,
                    BitsVotes = x.BitsVotes ?? 0
                }).ToArray()
            };

            Log.Information($"[Twitch Events] Channel poll end: {pollEndParams.BroadcasterChannelName} ({pollEndParams.BroadcasterChannelId}) - {pollEndParams.PollTitle}");

            await twitchEventHandlerService.HandlePollEndEvent(pollEndParams);
        }

        private async Task OnPollBegin(object sender, ChannelPollBeginArgs args)
        {
            var pollBeginParams = new ChannelPollBeginParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                PollTitle = args.Payload.Event.Title,
                PollChoices = args.Payload.Event.Choices.Select(x => new PollStartChoices
                {
                    Id = x.Id,
                    Title = x.Title
                }).ToArray()
            };

            Log.Information($"[Twitch Events] Channel poll begin: {pollBeginParams.BroadcasterChannelName} ({pollBeginParams.BroadcasterChannelId}) - {pollBeginParams.PollTitle}");

            await twitchEventHandlerService.HandlePollBeginEvent(pollBeginParams);
        }

        private async Task OnCustomRewardRedeemed(object sender, ChannelPointsCustomRewardArgs args)
        {
            Log.Information($"[Twitch Events] Channel custom reward redeemed: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId}) - {args.Payload.Event.Title}");
        }

        private async Task OnAutomaticRewardRedeemed(object sender, ChannelPointsAutomaticRewardRedemptionArgs args)
        {
            Log.Information($"[Twitch Events] Channel automatic reward redeemed: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId}) - {args.Payload.Event.Message}");
        }

        private async Task OnChannelRaid(object sender, ChannelRaidArgs args)
        {
            var raidParams = new ChannelRaidParams
            {
                BroadcasterChannelId = args.Payload.Event.ToBroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.ToBroadcasterUserName,
                RaidingChannelId = args.Payload.Event.FromBroadcasterUserId,
                RaidingChannelName = args.Payload.Event.FromBroadcasterUserName,
                Viewers = args.Payload.Event.Viewers
            };

            Log.Information($"[Twitch Events] Channel raid: {raidParams.RaidingChannelName} ({raidParams.RaidingChannelId}) raided {raidParams.BroadcasterChannelName} ({raidParams.BroadcasterChannelId}) with {raidParams.Viewers} viewers");

            await twitchEventHandlerService.HandleRaidEvent(raidParams);
        }

        private Task OnChannelUpdate(object sender, ChannelUpdateArgs args)
        {
            Log.Information($"[Twitch Events] Channel update: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId}). Title: {args.Payload.Event.Title} | Category: {args.Payload.Event.CategoryName}");
            return Task.CompletedTask;
        }

        private async Task OnChannelChatMessageReceived(object sender, ChannelChatMessageArgs args)
        {
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Payload.Event.ChatterUserId,
                ChatterChannelName = args.Payload.Event.ChatterUserName,
                Message = args.Payload.Event.Message.Text,
                MessageParts = args.Payload.Event.Message.Text.Split(' '),
                MessageId = args.Payload.Event.MessageId,
                IsMod = args.Payload.Event.IsModerator,
                IsSub = args.Payload.Event.IsSubscriber,
                IsVip = args.Payload.Event.IsVip,
                IsBroadcaster = args.Payload.Event.IsBroadcaster
            };

            await twitchHelperService.AddOrUpdateUserToDatabase(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId, msgParams.BroadcasterChannelName, msgParams.ChatterChannelName, msgParams.IsSub, msgParams.IsVip);
            await commandHandler.HandleCommandAsync(msgParams.Message.Split(' ')[0], msgParams);

            if (!msgParams.IsMod && !msgParams.IsBroadcaster)
            {
                await wordBlacklistMonitorService.CheckMessageForBlacklistedWords(msgParams.Message, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId);
            }

            await twitchHelperService.UpdateMessageCountForUser(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId, msgParams.ChatterChannelName, msgParams.Message);

            Log.Information($"[Twitch Events] Channel chat message: {msgParams.ChatterChannelName} ({msgParams.ChatterChannelId}) in {msgParams.BroadcasterChannelName} ({msgParams.BroadcasterChannelId}) - {msgParams.Message}");
        }

        private async Task OnSuspiciousUserMessage(object sender, ChannelSuspiciousUserMessageArgs args)
        {
            Log.Information($"[Twitch Events] Channel suspicious user message: {args.Payload.Event.UserName} ({args.Payload.Event.UserId}) in {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId}) - {args.Payload.Event.Message.Text}");

            await wordBlacklistMonitorService.CheckMessageForBlacklistedWords(args.Payload.Event.Message.Text, args.Payload.Event.UserId, args.Payload.Event.BroadcasterUserId);
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

        private async Task OnWebsocketDisconnected(object sender, EventArgs e, string twitchChannelName)
        {
            var userWebsocketConnection = _userConnections.GetValueOrDefault(twitchChannelName);

            if (userWebsocketConnection == null)
            {
                Log.Fatal($"[Twitch Events] Websocket disconnected for {twitchChannelName} but no connection found");
                return;
            }

            const int maxRetries = 5;
            int retryAttempt = 0;
            var random = new Random();

            while (retryAttempt < maxRetries)
            {
                Log.Warning($"[Twitch Events] Websocket disconnected for {twitchChannelName}. Attempting reconnect #{retryAttempt + 1}");

                try
                {
                    bool reconnected = await userWebsocketConnection.ReconnectAsync();

                    if (reconnected)
                    {
                        Log.Information($"[Twitch Events] Successfully reconnected websocket for {twitchChannelName} on attempt #{retryAttempt + 1}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Twitch Events] Reconnect attempt #{retryAttempt + 1} failed for {twitchChannelName}");
                }

                retryAttempt++;

                // Exponential backoff with jitter
                int delay = (int)(Math.Pow(2, retryAttempt) * 1000);
                int jitter = random.Next(0, 1000);
                int totalDelay = Math.Min(delay + jitter, 30000); // Cap at 30s

                Log.Warning($"[Twitch Events] Waiting {totalDelay}ms before next reconnect attempt for {twitchChannelName} 💤");
                await Task.Delay(totalDelay);
            }

            Log.Fatal($"[Twitch Events] Failed to reconnect websocket for {twitchChannelName} after {maxRetries} attempts 💀");
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
                        await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.suspicious_user.message", "1", new Dictionary<string, string>() { { "broadcaster_user_id", apiClient.BroadcasterChannelId }, { "moderator_user_id", apiClient.TwitchChannelClientId } }, EventSubTransportMethod.Websocket, userWebsocketConnection.SessionId);

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

#if DEBUG
            await twitchApiConnection.RefreshAllApiKeys();
#endif

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
                        userWebSocket.ChannelSuspiciousUserMessage += OnSuspiciousUserMessage;
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
                    userWebSocket.WebsocketDisconnected += (sender, e) => OnWebsocketDisconnected(sender, e, apiClient.TwitchUsername);
                    userWebSocket.WebsocketReconnected += OnWebsocketReconnected;
                    userWebSocket.ErrorOccurred += OnErrorOccurred;

                    await userWebSocket.ConnectAsync();
                }

                // check if the stream is already live. If it is then we need to fire the stream online event from the handler service
                if (apiClient.Type == AccountType.Broadcaster)
                {
                    Log.Information($"[Twitch API Connection] Checking if stream is live for {apiClient.TwitchUsername}");
                    var response = await twitchApiInteractionService.GetStreams(apiClient.ApiClient, apiClient.TwitchChannelClientId);

                    if (response != null)
                    {
                        Log.Information($"[Twitch API Connection] Stream is live for {apiClient.TwitchUsername}. Doing announcement stuff");

                        // check if stream has been up for more than 30 mins
                        var streamStartedMoreThan30MinsAgo = DateTime.UtcNow - response.StartedAt > TimeSpan.FromMinutes(30);
                        await twitchEventHandlerService.HandleStreamOnline(apiClient.BroadcasterChannelId, apiClient.BroadcasterChannelName, streamStartedMoreThan30MinsAgo);
                    }
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
