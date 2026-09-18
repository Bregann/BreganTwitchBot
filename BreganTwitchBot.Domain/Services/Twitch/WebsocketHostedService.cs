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
        /// <summary>
        /// The single websocket for the bot account. All chat/moderation events for every channel
        /// come down this one connection.
        /// </summary>
        private EventSubWebsocketClient? _botConnection;

        /// <summary>
        /// One websocket per broadcaster, keyed by the broadcaster's channel name. These are kept
        /// per channel because subscriptions, cheers, follows, polls and predictions are only
        /// available to that channel's own broadcaster token.
        /// </summary>
        private readonly Dictionary<string, EventSubWebsocketClient> _broadcasterConnections = [];

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
            twitchHelperService.ClearStreamChattersList(args.Payload.Event.BroadcasterUserId);
        }

        private async Task OnStreamOnline(object sender, StreamOnlineArgs args)
        {
            Log.Information($"[Twitch Events] Stream online: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId})");

            twitchHelperService.ClearStreamChattersList(args.Payload.Event.BroadcasterUserId);
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

            twitchHelperService.AddUserToStreamChattersList(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);
            twitchHelperService.IncrementChatMessageCount(msgParams.BroadcasterChannelId);

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

        private async Task OnWebsocketDisconnected(object sender, EventArgs e, string twitchChannelName, EventSubWebsocketClient? userWebsocketConnection)
        {
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


        /// <summary>
        /// The bot has connected. Subscribe to the bot level events for every channel the bot is
        /// active in - one websocket carries the chat and moderation events for all of them.
        /// </summary>
        private async Task OnBotWebsocketConnected(object sender, WebsocketConnectedArgs e)
        {
            if (e.IsRequestedReconnect)
            {
                return;
            }

            var botApiClient = twitchApiConnection.GetBotApiClient();

            if (botApiClient == null || _botConnection == null)
            {
                Log.Fatal("[Twitch API Connection] Bot websocket connected but there is no bot api client");
                return;
            }

            foreach (var channel in twitchApiConnection.GetAllChannels())
            {
                try
                {
                    // TODO: migrate to this when supported - channel.moderate
                    // await botApiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.moderate", "2", new Dictionary<string, string>() { { "broadcaster_user_id", channel.BroadcasterChannelId }, { "moderator_user_id", botApiClient.TwitchChannelClientId } }, EventSubTransportMethod.Websocket, _botConnection.SessionId);

                    // TODO: add unban requests when my PR is merged in
                    await botApiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.chat.message", "1", new Dictionary<string, string> { { "broadcaster_user_id", channel.BroadcasterChannelId }, { "user_id", botApiClient.TwitchChannelClientId } }, EventSubTransportMethod.Websocket, _botConnection.SessionId);
                    await botApiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.update", "2", new Dictionary<string, string> { { "broadcaster_user_id", channel.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, _botConnection.SessionId);
                    await botApiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.raid", "1", new Dictionary<string, string> { { "to_broadcaster_user_id", channel.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, _botConnection.SessionId);
                    await botApiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("stream.online", "1", new Dictionary<string, string> { { "broadcaster_user_id", channel.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, _botConnection.SessionId);
                    await botApiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("stream.offline", "1", new Dictionary<string, string> { { "broadcaster_user_id", channel.BroadcasterChannelId } }, EventSubTransportMethod.Websocket, _botConnection.SessionId);
                    await botApiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.suspicious_user.message", "1", new Dictionary<string, string> { { "broadcaster_user_id", channel.BroadcasterChannelId }, { "moderator_user_id", botApiClient.TwitchChannelClientId } }, EventSubTransportMethod.Websocket, _botConnection.SessionId);

                    Log.Information($"[Twitch API Connection] Subscribed to bot events in {channel.BroadcasterChannelName} as {botApiClient.TwitchUsername}");

                    await twitchHelperService.SendTwitchMessageToChannel(channel.BroadcasterChannelId, channel.BroadcasterChannelName, "hello currys (successfully connected)", null);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error subscribing to bot events in {channel.BroadcasterChannelName}");
                }
            }
        }

        /// <summary>
        /// A broadcaster has connected. These subscriptions need the channel's own broadcaster
        /// token as Twitch will not hand this data to the bot account.
        /// </summary>
        private async Task OnBroadcasterWebsocketConnected(object sender, WebsocketConnectedArgs e, string twitchChannelName)
        {
            if (e.IsRequestedReconnect)
            {
                return;
            }

            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(twitchChannelName);
            var broadcasterWebsocketConnection = _broadcasterConnections.GetValueOrDefault(twitchChannelName);

            if (apiClient == null || broadcasterWebsocketConnection == null)
            {
                return;
            }

            var broadcasterChannelId = apiClient.TwitchChannelClientId;

            try
            {
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.follow", "2", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId }, { "moderator_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscribe", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscription.gift", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.subscription.message", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.cheer", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.ban", "1", new Dictionary<string, string>() { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.unban", "1", new Dictionary<string, string>() { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.channel_points_automatic_reward_redemption.add", "2", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.channel_points_custom_reward_redemption.add", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.poll.begin", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.poll.end", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.begin", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.lock", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);
                await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.prediction.end", "1", new Dictionary<string, string> { { "broadcaster_user_id", broadcasterChannelId } }, EventSubTransportMethod.Websocket, broadcasterWebsocketConnection.SessionId);

                Log.Information($"[Twitch API Connection] Subscribed to broadcaster events for {apiClient.TwitchUsername}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error subscribing to events for {apiClient.TwitchUsername}");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            await twitchApiConnection.RefreshAllApiKeys();
#endif

            await StartBotConnectionAsync();
            await StartBroadcasterConnectionsAsync();
        }

        /// <summary>
        /// Opens the one and only bot websocket. Every channel's chat and moderation events arrive here.
        /// </summary>
        private async Task StartBotConnectionAsync()
        {
            var botApiClient = twitchApiConnection.GetBotApiClient();

            if (botApiClient == null)
            {
                Log.Fatal("[Twitch API Connection] No bot api client available, the bot will not connect to any channels");
                return;
            }

            if (_botConnection != null)
            {
                return;
            }

            _botConnection = new EventSubWebsocketClient();

            _botConnection.ChannelChatMessage += OnChannelChatMessageReceived;
            _botConnection.ChannelUpdate += OnChannelUpdate;
            _botConnection.ChannelRaid += OnChannelRaid;
            _botConnection.StreamOnline += OnStreamOnline;
            _botConnection.StreamOffline += OnStreamOffline;
            _botConnection.ChannelSuspiciousUserMessage += OnSuspiciousUserMessage;

            _botConnection.WebsocketConnected += OnBotWebsocketConnected;
            _botConnection.WebsocketDisconnected += (sender, e) => OnWebsocketDisconnected(sender, e, botApiClient.TwitchUsername, _botConnection);
            _botConnection.WebsocketReconnected += OnWebsocketReconnected;
            _botConnection.ErrorOccurred += OnErrorOccurred;

            await _botConnection.ConnectAsync();

            Log.Information($"[Twitch API Connection] Bot websocket connecting as {botApiClient.TwitchUsername}");
        }

        /// <summary>
        /// Opens a websocket per broadcaster. Needed alongside the bot connection because the
        /// subscription, cheer, follow, poll and prediction events are only granted to the
        /// channel's own broadcaster token.
        /// </summary>
        private async Task StartBroadcasterConnectionsAsync()
        {
            foreach (var apiClient in twitchApiConnection.GetAllBroadcasterApiClients())
            {
                if (!_broadcasterConnections.ContainsKey(apiClient.TwitchUsername))
                {
                    var broadcasterWebSocket = new EventSubWebsocketClient();
                    _broadcasterConnections.Add(apiClient.TwitchUsername, broadcasterWebSocket);

                    broadcasterWebSocket.ChannelBan += OnChannelBan;
                    broadcasterWebSocket.ChannelUnban += OnChannelUnban;
                    broadcasterWebSocket.ChannelPointsAutomaticRewardRedemptionAdd += OnAutomaticRewardRedeemed;
                    broadcasterWebSocket.ChannelPointsCustomRewardAdd += OnCustomRewardRedeemed;
                    broadcasterWebSocket.ChannelPollBegin += OnPollBegin;
                    broadcasterWebSocket.ChannelPollEnd += OnPollEnd;
                    broadcasterWebSocket.ChannelPredictionBegin += OnChannelPredictionBegin;
                    broadcasterWebSocket.ChannelPredictionLock += OnChannelPredictionLock;
                    broadcasterWebSocket.ChannelPredictionEnd += OnChannelPredictionEnd;
                    broadcasterWebSocket.ChannelFollow += OnFollowReceived;
                    broadcasterWebSocket.ChannelSubscribe += OnChannelSubcribe;
                    broadcasterWebSocket.ChannelSubscriptionGift += OnChannelSubscriptionGift;
                    broadcasterWebSocket.ChannelSubscriptionMessage += OnChannelResubscribe;
                    broadcasterWebSocket.ChannelCheer += OnChannelCheer;

                    broadcasterWebSocket.WebsocketConnected += (sender, e) => OnBroadcasterWebsocketConnected(sender, e, apiClient.TwitchUsername);
                    broadcasterWebSocket.WebsocketDisconnected += (sender, e) => OnWebsocketDisconnected(sender, e, apiClient.TwitchUsername, broadcasterWebSocket);
                    broadcasterWebSocket.WebsocketReconnected += OnWebsocketReconnected;
                    broadcasterWebSocket.ErrorOccurred += OnErrorOccurred;

                    await broadcasterWebSocket.ConnectAsync();
                }

                // check if the stream is already live. If it is then we need to fire the stream online event from the handler service
                Log.Information($"[Twitch API Connection] Checking if stream is live for {apiClient.TwitchUsername}");
                var response = await twitchApiInteractionService.GetStreams(apiClient.ApiClient, apiClient.TwitchChannelClientId);

                if (response != null)
                {
                    Log.Information($"[Twitch API Connection] Stream is live for {apiClient.TwitchUsername}. Doing announcement stuff");

                    var channelDetails = twitchApiConnection.GetChannelDetails(apiClient.TwitchChannelClientId);

                    // check if stream has been up for more than 30 mins
                    var streamStartedMoreThan30MinsAgo = DateTime.UtcNow - response.StartedAt > TimeSpan.FromMinutes(30);
                    await twitchEventHandlerService.HandleStreamOnline(apiClient.TwitchChannelClientId, channelDetails?.BroadcasterChannelName ?? apiClient.TwitchUsername, streamStartedMoreThan30MinsAgo);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_botConnection != null)
            {
                await _botConnection.DisconnectAsync();
            }

            foreach (var broadcaster in _broadcasterConnections)
            {
                await broadcaster.Value.DisconnectAsync();
            }
        }
    }
}
