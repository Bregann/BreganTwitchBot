using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch.Events;
using Microsoft.Extensions.Hosting;
using Serilog;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.EventSub;
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
        ITwitchApiInteractionService twitchApiInteractionService,
        IStreamStatsService streamStatsService
    ) : IHostedService, IWebsocketHostedService
    {
        /// <summary>
        /// The single websocket for the bot account. All chat/moderation events for every channel
        /// come down this one connection.
        /// </summary>
        private TrackedConnection? _botConnection;

        /// <summary>
        /// One websocket per broadcaster, keyed by the broadcaster's channel name. These are kept
        /// per channel because subscriptions, cheers, follows, polls and predictions are only
        /// available to that channel's own broadcaster token.
        /// </summary>
        private readonly Dictionary<string, TrackedConnection> _broadcasterConnections = [];

        /// <summary>
        /// Cancelled when the bot is shutting down so any reconnect loops stop
        /// </summary>
        private readonly CancellationTokenSource _stoppingCts = new();

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

            streamStatsService.UpdateStreamStat(args.Payload.Event.BroadcasterUserId, StreamStatType.NewFollowers);
            await twitchHelperService.AddOrUpdateUserToDatabase(args.Payload.Event.BroadcasterUserId, args.Payload.Event.UserId, args.Payload.Event.BroadcasterUserName, args.Payload.Event.UserName);
        }

        private async Task OnStreamOffline(object sender, StreamOfflineArgs args)
        {
            Log.Information($"[Twitch Events] Stream offline: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId})");
            await twitchEventHandlerService.HandleStreamOffline(args.Payload.Event.BroadcasterUserId, args.Payload.Event.BroadcasterUserName);
        }

        private async Task OnStreamOnline(object sender, StreamOnlineArgs args)
        {
            Log.Information($"[Twitch Events] Stream online: {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId})");
            await twitchEventHandlerService.HandleStreamOnline(args.Payload.Event.BroadcasterUserId, args.Payload.Event.BroadcasterUserName, args.Payload.Event.Id, args.Payload.Event.StartedAt.UtcDateTime);
        }

        private async Task OnChannelBan(object sender, ChannelBanArgs args)
        {
            Log.Information($"[Twitch Events] Channel ban: {args.Payload.Event.UserName} ({args.Payload.Event.UserId}) in {args.Payload.Event.BroadcasterUserName} ({args.Payload.Event.BroadcasterUserId})");

            // twitch models a timeout as a ban with an end date
            streamStatsService.UpdateStreamStat(args.Payload.Event.BroadcasterUserId, args.Payload.Event.EndsAt == null ? StreamStatType.TotalBans : StreamStatType.TotalTimeouts);
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

        private async Task OnCustomRewardRedeemed(object sender, ChannelPointsCustomRewardRedemptionArgs args)
        {
            var redeemedParams = new ChannelPointsRedeemedParams
            {
                BroadcasterChannelId = args.Payload.Event.BroadcasterUserId,
                BroadcasterChannelName = args.Payload.Event.BroadcasterUserName,
                ChatterChannelId = args.Payload.Event.UserId,
                ChatterChannelName = args.Payload.Event.UserName,
                RewardId = args.Payload.Event.Reward.Id,
                RewardTitle = args.Payload.Event.Reward.Title,
                RewardCost = args.Payload.Event.Reward.Cost,
                RedemptionStatus = args.Payload.Event.Status,
                UserInput = args.Payload.Event.UserInput
            };

            await twitchEventHandlerService.HandleChannelPointsRedeemedEvent(redeemedParams);
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

            streamStatsService.UpdateStreamStat(msgParams.BroadcasterChannelId, StreamStatType.MessagesReceived);
            streamStatsService.AddUniqueViewer(msgParams.BroadcasterChannelId, msgParams.ChatterChannelName);

            if (msgParams.Message.StartsWith('!'))
            {
                streamStatsService.UpdateStreamStat(msgParams.BroadcasterChannelId, StreamStatType.CommandsSent);
            }

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

        /// <summary>
        /// Twitch asked us to move to a new server and the library handled it. Subscriptions carry
        /// over to the new socket so there's nothing to do.
        /// </summary>
        private Task OnWebsocketReconnected(TrackedConnection connection)
        {
            Log.Information($"[Twitch Events] Websocket for {connection.Name} reconnected at Twitch's request");
            return Task.CompletedTask;
        }

        private Task OnWebsocketDisconnected(EventSubWebsocketClient client, TrackedConnection connection)
        {
            // an old client we've already replaced
            if (client != connection.Client)
            {
                return Task.CompletedTask;
            }

            Log.Warning($"[Twitch Events] Websocket disconnected for {connection.Name}");

            // don't hold up the library's connection check task with the reconnect loop
            _ = Task.Run(() => ConnectWithRetryAsync(connection));
            return Task.CompletedTask;
        }

        private async Task OnWebsocketConnected(EventSubWebsocketClient client, WebsocketConnectedArgs e, TrackedConnection connection)
        {
            if (client != connection.Client || e.IsRequestedReconnect)
            {
                return;
            }

            var activeSubscriptions = 0;

            try
            {
                activeSubscriptions = await SyncSubscriptionsAsync(connection, client, isNewSession: true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[Twitch Events] Error subscribing to events on the {connection.Name} websocket");
            }

            // twitch closes a websocket with no subscriptions after a few seconds, so count that as a failure
            connection.Welcomed?.TrySetResult(activeSubscriptions > 0);
        }

        /// <summary>
        /// Keeps trying to get a connection up until it works or the bot is stopping. Only one of
        /// these runs per connection at a time - a dying socket can raise disconnected more than once.
        /// </summary>
        private async Task ConnectWithRetryAsync(TrackedConnection connection)
        {
            if (!await connection.ReconnectLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                var attempt = 0;

                while (!_stoppingCts.IsCancellationRequested)
                {
                    attempt++;
                    Log.Information($"[Twitch Events] Connecting websocket for {connection.Name}. Attempt #{attempt}");

                    try
                    {
                        if (await TryConnectAsync(connection))
                        {
                            Log.Information($"[Twitch Events] Websocket connected and subscribed for {connection.Name} on attempt #{attempt}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"[Twitch Events] Connect attempt #{attempt} failed for {connection.Name}");
                    }

                    // Exponential backoff with jitter, capped at 60s. Never give up - a long outage should still recover
                    var delay = Math.Min(Math.Pow(2, Math.Min(attempt, 6)) * 1000, 60000) + Random.Shared.Next(0, 1000);

                    Log.Warning($"[Twitch Events] Waiting {delay}ms before next connect attempt for {connection.Name} 💤");

                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(delay), _stoppingCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
            finally
            {
                connection.ReconnectLock.Release();
            }
        }

        /// <summary>
        /// Swaps in a brand new client rather than calling ReconnectAsync on the old one. The library
        /// can be left stuck thinking a Twitch requested reconnect is in progress, which then stops
        /// the new session from ever being subscribed to.
        /// </summary>
        private async Task<bool> TryConnectAsync(TrackedConnection connection)
        {
            var oldClient = connection.Client;
            connection.Client = null;

            if (oldClient != null)
            {
                try
                {
                    await oldClient.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, $"[Twitch Events] Error disconnecting the old websocket for {connection.Name}");
                }
            }

            var welcomed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            connection.Welcomed = welcomed;

            var client = CreateClient(connection);
            connection.Client = client;

            if (!await client.ConnectAsync())
            {
                Log.Warning($"[Twitch Events] Websocket for {connection.Name} failed to open");
                return false;
            }

            // the socket being open isn't enough - wait for twitch's welcome and the subscriptions to go in
            var finished = await Task.WhenAny(welcomed.Task, Task.Delay(TimeSpan.FromSeconds(30), _stoppingCts.Token));

            if (finished != welcomed.Task)
            {
                Log.Warning($"[Twitch Events] Websocket for {connection.Name} opened but was never welcomed by Twitch");
                return false;
            }

            return welcomed.Task.Result;
        }

        private EventSubWebsocketClient CreateClient(TrackedConnection connection)
        {
            var client = new EventSubWebsocketClient();

            if (connection.IsBot)
            {
                client.ChannelChatMessage += OnChannelChatMessageReceived;
                client.ChannelUpdate += OnChannelUpdate;
                client.ChannelRaid += OnChannelRaid;
                client.StreamOnline += OnStreamOnline;
                client.StreamOffline += OnStreamOffline;
                client.ChannelSuspiciousUserMessage += OnSuspiciousUserMessage;
            }
            else
            {
                client.ChannelBan += OnChannelBan;
                client.ChannelUnban += OnChannelUnban;
                client.ChannelPointsAutomaticRewardRedemptionAdd += OnAutomaticRewardRedeemed;
                client.ChannelPointsCustomRewardRedemptionAdd += OnCustomRewardRedeemed;
                client.ChannelPollBegin += OnPollBegin;
                client.ChannelPollEnd += OnPollEnd;
                client.ChannelPredictionBegin += OnChannelPredictionBegin;
                client.ChannelPredictionLock += OnChannelPredictionLock;
                client.ChannelPredictionEnd += OnChannelPredictionEnd;
                client.ChannelFollow += OnFollowReceived;
                client.ChannelSubscribe += OnChannelSubcribe;
                client.ChannelSubscriptionGift += OnChannelSubscriptionGift;
                client.ChannelSubscriptionMessage += OnChannelResubscribe;
                client.ChannelCheer += OnChannelCheer;
            }

            client.WebsocketConnected += (sender, e) => OnWebsocketConnected(client, e, connection);
            client.WebsocketDisconnected += (sender, e) => OnWebsocketDisconnected(client, connection);
            client.WebsocketReconnected += (sender, e) => OnWebsocketReconnected(connection);
            client.ErrorOccurred += OnErrorOccurred;

            return client;
        }

        /// <summary>
        /// The bot level events for every channel the bot is active in - one websocket carries the
        /// chat and moderation events for all of them.
        /// </summary>
        private List<DesiredSubscription> GetBotSubscriptions(TwitchApiConnection.TwitchAccount botApiClient)
        {
            var botUserId = botApiClient.TwitchChannelClientId;
            var subscriptions = new List<DesiredSubscription>();

            foreach (var channel in twitchApiConnection.GetAllChannels())
            {
                var name = channel.BroadcasterChannelName;
                var id = channel.BroadcasterChannelId;

                // TODO: migrate to this when supported - channel.moderate
                // new(name, id, "channel.moderate", "2", new() { { "broadcaster_user_id", id }, { "moderator_user_id", botUserId } })

                // TODO: add unban requests when my PR is merged in
                subscriptions.AddRange(
                [
                    new(name, id, "channel.chat.message", "1", new() { { "broadcaster_user_id", id }, { "user_id", botUserId } }),
                    new(name, id, "channel.update", "2", new() { { "broadcaster_user_id", id } }),
                    new(name, id, "channel.raid", "1", new() { { "to_broadcaster_user_id", id } }),
                    new(name, id, "stream.online", "1", new() { { "broadcaster_user_id", id } }),
                    new(name, id, "stream.offline", "1", new() { { "broadcaster_user_id", id } }),
                    new(name, id, "channel.suspicious_user.message", "1", new() { { "broadcaster_user_id", id }, { "moderator_user_id", botUserId } })
                ]);
            }

            return subscriptions;
        }

        /// <summary>
        /// These subscriptions need the channel's own broadcaster token as Twitch will not hand this
        /// data to the bot account.
        /// </summary>
        private static List<DesiredSubscription> GetBroadcasterSubscriptions(TwitchApiConnection.TwitchAccount apiClient)
        {
            var name = apiClient.TwitchUsername;
            var id = apiClient.TwitchChannelClientId;

            return
            [
                new(name, id, "channel.follow", "2", new() { { "broadcaster_user_id", id }, { "moderator_user_id", id } }),
                new(name, id, "channel.subscribe", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.subscription.gift", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.subscription.message", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.cheer", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.ban", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.unban", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.channel_points_automatic_reward_redemption.add", "2", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.channel_points_custom_reward_redemption.add", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.poll.begin", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.poll.end", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.prediction.begin", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.prediction.lock", "1", new() { { "broadcaster_user_id", id } }),
                new(name, id, "channel.prediction.end", "1", new() { { "broadcaster_user_id", id } })
            ];
        }

        /// <summary>
        /// Creates any of the connection's subscriptions that Twitch doesn't have for the current
        /// session. Each one is tried on its own so a single failure doesn't stop the rest.
        /// Returns how many subscriptions are active once done.
        /// </summary>
        private async Task<int> SyncSubscriptionsAsync(TrackedConnection connection, EventSubWebsocketClient client, bool isNewSession)
        {
            var apiClient = connection.IsBot ? twitchApiConnection.GetBotApiClient() : twitchApiConnection.GetBroadcasterApiClientFromChannelName(connection.Name);

            if (apiClient == null)
            {
                Log.Fatal($"[Twitch API Connection] Websocket for {connection.Name} connected but there is no api client");
                return 0;
            }

            var sessionId = client.SessionId;
            var desired = connection.IsBot ? GetBotSubscriptions(apiClient) : GetBroadcasterSubscriptions(apiClient);
            var missing = desired;

            // a brand new session never has any subscriptions, so only ask twitch when checking an existing one
            if (!isNewSession)
            {
                var existing = await GetEnabledSubscriptionsAsync(apiClient, sessionId);
                missing = desired.Where(x => !existing.Any(y => IsSameSubscription(x, y))).ToList();

                if (missing.Count == 0)
                {
                    return desired.Count;
                }

                Log.Warning($"[Twitch Events] {missing.Count} subscription(s) missing on the {connection.Name} websocket, resubscribing");
            }

            var failed = new List<DesiredSubscription>();

            foreach (var subscription in missing)
            {
                try
                {
                    await apiClient.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync(subscription.Type, subscription.Version, subscription.Condition, EventSubTransportMethod.Websocket, sessionId);
                }
                catch (Exception ex)
                {
                    failed.Add(subscription);
                    Log.Error(ex, $"[Twitch Events] Error subscribing to {subscription.Type} in {subscription.ChannelName} on the {connection.Name} websocket");
                }
            }

            foreach (var channel in missing.GroupBy(x => (x.ChannelName, x.BroadcasterChannelId)))
            {
                var channelFailures = failed.Count(x => x.BroadcasterChannelId == channel.Key.BroadcasterChannelId);

                Log.Information($"[Twitch API Connection] Subscribed to {channel.Count() - channelFailures}/{channel.Count()} events in {channel.Key.ChannelName} as {apiClient.TwitchUsername}");

                if (isNewSession && connection.IsBot && channelFailures == 0)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(channel.Key.BroadcasterChannelId, channel.Key.ChannelName, "hello currys (successfully connected)", null);
                }
            }

            return desired.Count - failed.Count;
        }

        private static async Task<List<EventSubSubscription>> GetEnabledSubscriptionsAsync(TwitchApiConnection.TwitchAccount apiClient, string sessionId)
        {
            var subscriptions = new List<EventSubSubscription>();
            string? after = null;

            do
            {
                var response = await apiClient.ApiClient.Helix.EventSub.GetEventSubSubscriptionsAsync(status: "enabled", after: after);

                subscriptions.AddRange(response.Subscriptions.Where(x => x.Transport?.SessionId == sessionId));
                after = response.Pagination?.Cursor;
            }
            while (!string.IsNullOrEmpty(after));

            return subscriptions;
        }

        private static bool IsSameSubscription(DesiredSubscription desired, EventSubSubscription existing)
        {
            return existing.Type == desired.Type && desired.Condition.All(x => existing.Condition != null && existing.Condition.TryGetValue(x.Key, out var value) && value == x.Value);
        }

        /// <summary>
        /// Safety net run on a schedule. Recreates any subscriptions Twitch has lost or that failed
        /// to go in, and reconnects any websocket that has died without us noticing.
        /// </summary>
        public async Task VerifyConnectionsAsync()
        {
            var connections = _broadcasterConnections.Values.ToList();

            if (_botConnection != null)
            {
                connections.Insert(0, _botConnection);
            }

            foreach (var connection in connections)
            {
                // a connect is already running for this one
                if (!await connection.ReconnectLock.WaitAsync(0))
                {
                    continue;
                }

                var needsReconnect = false;

                try
                {
                    var client = connection.Client;

                    needsReconnect = client == null || string.IsNullOrEmpty(client.SessionId) || await SyncSubscriptionsAsync(connection, client, isNewSession: false) == 0;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Twitch Events] Error verifying the {connection.Name} websocket");
                }
                finally
                {
                    connection.ReconnectLock.Release();
                }

                if (needsReconnect)
                {
                    Log.Warning($"[Twitch Events] Websocket for {connection.Name} has no active subscriptions, reconnecting");
                    _ = Task.Run(() => ConnectWithRetryAsync(connection));
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            await twitchApiConnection.RefreshAllApiKeys();
#endif

            StartBotConnection();
            await StartBroadcasterConnectionsAsync();
        }

        /// <summary>
        /// Opens the one and only bot websocket. Every channel's chat and moderation events arrive here.
        /// </summary>
        private void StartBotConnection()
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

            _botConnection = new TrackedConnection(botApiClient.TwitchUsername, isBot: true);

            // connects in the background so a twitch outage doesn't hold up the rest of the app starting
            _ = Task.Run(() => ConnectWithRetryAsync(_botConnection));

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
                    var connection = new TrackedConnection(apiClient.TwitchUsername, isBot: false);
                    _broadcasterConnections.Add(apiClient.TwitchUsername, connection);

                    _ = Task.Run(() => ConnectWithRetryAsync(connection));
                }

                // catch up on the stream starting or ending while the bot was down. This also runs every
                // minute, for anything missed while the bot was disconnected from twitch
                Log.Information($"[Twitch API Connection] Checking the stream status for {apiClient.TwitchUsername}");
                var liveStream = await twitchApiInteractionService.GetStreams(apiClient.ApiClient, apiClient.TwitchChannelClientId);
                var channelDetails = twitchApiConnection.GetChannelDetails(apiClient.TwitchChannelClientId);

                await twitchEventHandlerService.CheckStreamStatus(apiClient.TwitchChannelClientId, channelDetails?.BroadcasterChannelName ?? apiClient.TwitchUsername, liveStream);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingCts.Cancel();

            var connections = _broadcasterConnections.Values.ToList();

            if (_botConnection != null)
            {
                connections.Add(_botConnection);
            }

            foreach (var connection in connections)
            {
                if (connection.Client != null)
                {
                    await connection.Client.DisconnectAsync();
                }
            }
        }

        private record DesiredSubscription(string ChannelName, string BroadcasterChannelId, string Type, string Version, Dictionary<string, string> Condition);

        /// <summary>
        /// A websocket we keep alive - either the bot's or a broadcaster's. The client inside gets
        /// swapped for a fresh one on every reconnect.
        /// </summary>
        private sealed class TrackedConnection(string name, bool isBot)
        {
            public string Name { get; } = name;
            public bool IsBot { get; } = isBot;
            public EventSubWebsocketClient? Client { get; set; }

            /// <summary>
            /// Completed once Twitch has welcomed the current client and the subscriptions have gone in
            /// </summary>
            public TaskCompletionSource<bool>? Welcomed { get; set; }

            public SemaphoreSlim ReconnectLock { get; } = new(1, 1);
        }
    }
}
