using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch.Events;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TwitchLib.Api.Core.Enums;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    public class WebsocketHostedService(
        ITwitchApiConnection twitchApiConnection,
        ICommandHandler commandHandler,
        ITwitchHelperService twitchHelperService,
        ITwitchEventHandlerService twitchEventHandlerService,
        IConfigHelperService configHelperService,
        IWordBlacklistMonitorService wordBlacklistMonitorService,
        IServiceProvider serviceProvider,
        IDiscordHelperService discordHelperService
    ) : IHostedService
    {
        private readonly Dictionary<string, EventSubWebsocketClient> _userConnections = [];

        private async Task OnChannelCheer(object sender, ChannelCheerArgs args)
        {
            var bitsEvent = new BitsCheeredParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Notification.Payload.Event.UserId ?? "Anon",
                ChatterChannelName = args.Notification.Payload.Event.UserName ?? "Anon",
                Amount = args.Notification.Payload.Event.Bits,
                Message = args.Notification.Payload.Event.Message,
                IsAnonymous = args.Notification.Payload.Event.IsAnonymous
            };

            Log.Information($"[Twitch Events] Bits cheered: {bitsEvent.Amount} from {bitsEvent.ChatterChannelName} in {bitsEvent.BroadcasterChannelName}");

            await twitchEventHandlerService.HandleChannelCheerEvent(bitsEvent);
        }

        private async Task OnChannelResubscribe(object sender, ChannelSubscriptionMessageArgs args)
        {
            var resubEvent = new ChannelResubscribeParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Notification.Payload.Event.UserId,
                ChatterChannelName = args.Notification.Payload.Event.UserName,
                Message = args.Notification.Payload.Event.Message.Text,
                StreakMonths = args.Notification.Payload.Event.StreakMonths,
                CumulativeMonths = args.Notification.Payload.Event.CumulativeMonths,
                SubTier = Enum.TryParse<SubTierEnum>(args.Notification.Payload.Event.Tier, out var tier) ? tier : SubTierEnum.Tier1
            };

            Log.Information($"[Twitch Events] Channel resubscribe: {resubEvent.ChatterChannelName} in {resubEvent.BroadcasterChannelName}. Tier: {resubEvent.SubTier}");

            await twitchEventHandlerService.HandleChannelResubscribeEvent(resubEvent);
        }

        private async Task OnChannelSubscriptionGift(object sender, ChannelSubscriptionGiftArgs args)
        {
            var giftSubEvent = new ChannelGiftSubParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Notification.Payload.Event.UserId,
                ChatterChannelName = args.Notification.Payload.Event.UserName,
                IsAnonymous = args.Notification.Payload.Event.IsAnonymous,
                SubTier = Enum.TryParse<SubTierEnum>(args.Notification.Payload.Event.Tier, out var tier) ? tier : SubTierEnum.Tier1,
                CumulativeTotal = args.Notification.Payload.Event.CumulativeTotal,
                Total = args.Notification.Payload.Event.Total
            };

            Log.Information($"[Twitch Events] Channel gift sub: {giftSubEvent.ChatterChannelName} in {giftSubEvent.BroadcasterChannelName}. Tier: {giftSubEvent.SubTier}");

            await twitchEventHandlerService.HandleChannelGiftSubEvent(giftSubEvent);
        }

        private async Task OnChannelSubcribe(object sender, ChannelSubscribeArgs args)
        {
            var subEvent = new ChannelSubscribeParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Notification.Payload.Event.UserId,
                ChatterChannelName = args.Notification.Payload.Event.UserName,
                SubTier = Enum.TryParse<SubTierEnum>(args.Notification.Payload.Event.Tier, out var tier) ? tier : SubTierEnum.Tier1,
                IsGift = args.Notification.Payload.Event.IsGift
            };

            Log.Information($"[Twitch Events] Channel subscribe: {subEvent.ChatterChannelName} in {subEvent.BroadcasterChannelName}. Tier: {subEvent.SubTier}");

            await twitchEventHandlerService.HandleChannelSubEvent(subEvent);
        }

        private async Task OnFollowReceived(object sender, ChannelFollowArgs args)
        {
            Log.Information($"[Twitch Events] Channel follow: {args.Notification.Payload.Event.UserName} ({args.Notification.Payload.Event.UserId}) in {args.Notification.Payload.Event.BroadcasterUserName}");
            await twitchHelperService.AddOrUpdateUserToDatabase(args.Notification.Payload.Event.BroadcasterUserId, args.Notification.Payload.Event.UserId, args.Notification.Payload.Event.BroadcasterUserName, args.Notification.Payload.Event.UserName);
        }

        private async Task OnStreamOffline(object sender, TwitchLib.EventSub.Websockets.Core.EventArgs.Stream.StreamOfflineArgs args)
        {
            Log.Information($"[Twitch Events] Stream offline: {args.Notification.Payload.Event.BroadcasterUserName} ({args.Notification.Payload.Event.BroadcasterUserId})");

            await configHelperService.UpdateStreamLiveStatus(args.Notification.Payload.Event.BroadcasterUserId, false);
        }

        private async Task OnStreamOnline(object sender, TwitchLib.EventSub.Websockets.Core.EventArgs.Stream.StreamOnlineArgs args)
        {
            Log.Information($"[Twitch Events] Stream online: {args.Notification.Payload.Event.BroadcasterUserName} ({args.Notification.Payload.Event.BroadcasterUserId})");

            await configHelperService.UpdateStreamLiveStatus(args.Notification.Payload.Event.BroadcasterUserId, true);
            var discordEnabled = configHelperService.IsDiscordEnabled(args.Notification.Payload.Event.BroadcasterUserId);

            if (discordEnabled)
            {
                var discordConfig = configHelperService.GetDiscordConfig(args.Notification.Payload.Event.BroadcasterUserId);
                if (discordConfig != null && discordConfig.DiscordStreamAnnouncementChannelId != null)
                {
                    await discordHelperService.SendMessage(discordConfig.DiscordStreamAnnouncementChannelId.Value, $"Hey @everyone !!! {args.Notification.Payload.Event.BroadcasterUserName} is now live!! Woooo! Tune in at https://twitch.tv/{args.Notification.Payload.Event.BroadcasterUserName.ToLower()}");
                }
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var dailyPointsDataService = scope.ServiceProvider.GetRequiredService<IDailyPointsDataService>();

                await dailyPointsDataService.ScheduleDailyPointsCollection(args.Notification.Payload.Event.BroadcasterUserId);

                BackgroundJob.Schedule<ITwitchBossesDataService>(svc =>
                    svc.StartBossFightCountdown(
                        args.Notification.Payload.Event.BroadcasterUserId,
                        args.Notification.Payload.Event.BroadcasterUserName,
                        null),
                    TimeSpan.FromMinutes(45));
            }
        }

        private async Task OnChannelBan(object sender, ChannelBanArgs args)
        {
            Log.Information($"[Twitch Events] Channel ban: {args.Notification.Payload.Event.UserName} ({args.Notification.Payload.Event.UserId}) in {args.Notification.Payload.Event.BroadcasterUserName} ({args.Notification.Payload.Event.BroadcasterUserId})");
        }

        private async Task OnChannelUnban(object sender, ChannelUnbanArgs args)
        {
            Log.Information($"[Twitch Events] Channel unban: {args.Notification.Payload.Event.UserName} ({args.Notification.Payload.Event.UserId}) in {args.Notification.Payload.Event.BroadcasterUserName}");
        }

        private async Task OnChannelPredictionEnd(object sender, ChannelPredictionEndArgs args)
        {
            var predictionEndParams = new ChannelPredictionEndParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                PredictionId = args.Notification.Payload.Event.Id,
                WonOutcome = args.Notification.Payload.Event.Outcomes.Where(x => x.Id == args.Notification.Payload.Event.WinningOutcomeId).Select(x => new ChannelPredictionEndParams.Outcome
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
                PredictionStatus = args.Notification.Payload.Event.Status,
                PredictionTitle = args.Notification.Payload.Event.Title
            };

            Log.Information($"[Twitch Events] Channel prediction end: {predictionEndParams.BroadcasterChannelName} ({predictionEndParams.BroadcasterChannelId}) - {predictionEndParams.PredictionTitle}");

            await twitchEventHandlerService.HandlePredictionEndEvent(predictionEndParams);
        }

        private async Task OnChannelPredictionLock(object sender, ChannelPredictionLockArgs args)
        {
            var predictionLockParams = new ChannelPredictionLockedParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                PredictionId = args.Notification.Payload.Event.Id,
                PredictionTitle = args.Notification.Payload.Event.Title,
                PredictionStatus = "Locked"
            };

            Log.Information($"[Twitch Events] Channel prediction lock: {predictionLockParams.BroadcasterChannelName} ({predictionLockParams.BroadcasterChannelId}) - {predictionLockParams.PredictionTitle}");

            await twitchEventHandlerService.HandlePredictionLockedEvent(predictionLockParams);
        }

        private async Task OnChannelPredictionBegin(object sender, ChannelPredictionBeginArgs args)
        {
            var predictionBeginParams = new ChannelPredictionBeginParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                PredictionId = args.Notification.Payload.Event.Id,
                PredictionTitle = args.Notification.Payload.Event.Title,
                PredictionStatus = "Started",
                PredictionOutcomeOptions = args.Notification.Payload.Event.Outcomes.Select(x => new PredictionOutcomeOption
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
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                PollTitle = args.Notification.Payload.Event.Title,
                PollEndResults = args.Notification.Payload.Event.Choices.Select(x => new PollEndChoices
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
                BroadcasterChannelId = args.Notification.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.BroadcasterUserName,
                PollTitle = args.Notification.Payload.Event.Title,
                PollChoices = args.Notification.Payload.Event.Choices.Select(x => new PollStartChoices
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
            Log.Information($"[Twitch Events] Channel custom reward redeemed: {args.Notification.Payload.Event.BroadcasterUserName} ({args.Notification.Payload.Event.BroadcasterUserId}) - {args.Notification.Payload.Event.Title}");
        }

        private async Task OnAutomaticRewardRedeemed(object sender, ChannelPointsAutomaticRewardRedemptionArgs args)
        {
            Log.Information($"[Twitch Events] Channel automatic reward redeemed: {args.Notification.Payload.Event.BroadcasterUserName} ({args.Notification.Payload.Event.BroadcasterUserId}) - {args.Notification.Payload.Event.Message}");
        }

        private async Task OnChannelRaid(object sender, ChannelRaidArgs args)
        {
            var raidParams = new ChannelRaidParams
            {
                BroadcasterChannelId = args.Notification.Payload.Event.ToBroadcasterUserId,
                BroadcasterChannelName = args.Notification.Payload.Event.ToBroadcasterUserName,
                RaidingChannelId = args.Notification.Payload.Event.FromBroadcasterUserId,
                RaidingChannelName = args.Notification.Payload.Event.FromBroadcasterUserName,
                Viewers = args.Notification.Payload.Event.Viewers
            };

            Log.Information($"[Twitch Events] Channel raid: {raidParams.RaidingChannelName} ({raidParams.RaidingChannelId}) raided {raidParams.BroadcasterChannelName} ({raidParams.BroadcasterChannelId}) with {raidParams.Viewers} viewers");

            await twitchEventHandlerService.HandleRaidEvent(raidParams);
        }

        private Task OnChannelUpdate(object sender, ChannelUpdateArgs args)
        {
            Log.Information($"[Twitch Events] Channel update: {args.Notification.Payload.Event.BroadcasterUserName} ({args.Notification.Payload.Event.BroadcasterUserId}). Title: {args.Notification.Payload.Event.Title} | Category: {args.Notification.Payload.Event.CategoryName}");
            return Task.CompletedTask;
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

            await twitchHelperService.AddOrUpdateUserToDatabase(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId, msgParams.BroadcasterChannelName, msgParams.ChatterChannelName, msgParams.IsSub, msgParams.IsVip);
            await commandHandler.HandleCommandAsync(msgParams.Message.Split(' ')[0], msgParams);

            if (!msgParams.IsMod && !msgParams.IsBroadcaster)
            {
                await wordBlacklistMonitorService.CheckMessageForBlacklistedWords(msgParams.Message, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId);
            }

            await twitchHelperService.UpdateMessageCountForUser(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId, msgParams.ChatterChannelName, msgParams.Message);

            Log.Information($"[Twitch Events] Channel chat message: {msgParams.ChatterChannelName} ({msgParams.ChatterChannelId}) in {msgParams.BroadcasterChannelName} ({msgParams.BroadcasterChannelId}) - {msgParams.Message}");
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
                    userWebSocket.WebsocketDisconnected += (sender, e) => OnWebsocketDisconnected(sender, e, apiClient.TwitchUsername);
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
