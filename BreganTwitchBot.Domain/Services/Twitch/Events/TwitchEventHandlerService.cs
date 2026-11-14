using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Helpers;
using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch.Events;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services.Twitch.Events
{
    public class TwitchEventHandlerService(
        ITwitchHelperService twitchHelperService,
        ITwitchApiInteractionService twitchApiInteractionService,
        ITwitchApiConnection twitchApiConnection,
        IConfigHelperService configHelperService,
        IDiscordHelperService discordHelperService,
        IServiceProvider serviceProvider,
        IStreamStatsService streamStatsService
        ) : ITwitchEventHandlerService
    {
        public async Task HandleChannelCheerEvent(BitsCheeredParams cheerParams)
        {
            streamStatsService.UpdateStreamStat(cheerParams.BroadcasterChannelId, StreamStatType.BitsDonated, cheerParams.Amount);

            await twitchHelperService.SendTwitchMessageToChannel(cheerParams.BroadcasterChannelId, cheerParams.BroadcasterChannelName, $"Thank you for the {cheerParams.Amount} bits, {cheerParams.ChatterChannelName}! PogChamp");

            // anonymous cheers have no user to credit the contribution to
            if (!cheerParams.IsAnonymous)
            {
                await AddSubathonTime(x => x.AddBitsTime(cheerParams.BroadcasterChannelId, cheerParams.ChatterChannelId, cheerParams.Amount));

                // anonymous cheers have nobody to credit on the monthly leaderboard
                await UpdateMonthlyTotals(cheerParams.BroadcasterChannelId, cheerParams.ChatterChannelId, bitsDonated: cheerParams.Amount, subsGifted: 0);
            }
        }

        public async Task HandleChannelResubscribeEvent(ChannelResubscribeParams resubscribeParams)
        {
            var pointsToAdd = resubscribeParams.SubTier switch
            {
                SubTierEnum.Tier1 => 20000 + (resubscribeParams.CumulativeMonths * 1500) + (resubscribeParams.StreakMonths ?? 0 * 3000),
                SubTierEnum.Tier2 => 40000 + (resubscribeParams.CumulativeMonths * 3000) + (resubscribeParams.StreakMonths ?? 0 * 3000),
                SubTierEnum.Tier3 => 60000 + (resubscribeParams.CumulativeMonths * 6000) + (resubscribeParams.StreakMonths ?? 0 * 3000),
                _ => 0
            };

            await twitchHelperService.AddPointsToUser(resubscribeParams.BroadcasterChannelId, resubscribeParams.ChatterChannelId, pointsToAdd, resubscribeParams.BroadcasterChannelName, resubscribeParams.ChatterChannelName);

            var streakMessage = resubscribeParams.StreakMonths > 0 ? $"They are on a {resubscribeParams.StreakMonths} month sub streak! <3" : "they did not share their sub streak :(";
            await twitchHelperService.SendTwitchMessageToChannel(resubscribeParams.BroadcasterChannelId, resubscribeParams.BroadcasterChannelName, $"Thank you for the {resubscribeParams.CumulativeMonths} month resubscription, {resubscribeParams.ChatterChannelName}! {streakMessage} <3");
        }

        public async Task HandleChannelGiftSubEvent(ChannelGiftSubParams giftSubParams)
        {
            streamStatsService.UpdateStreamStat(giftSubParams.BroadcasterChannelId, StreamStatType.NewGiftedSubs, giftSubParams.Total);

            var pointsToAdd = giftSubParams.SubTier switch
            {
                SubTierEnum.Tier1 => 20000,
                SubTierEnum.Tier2 => 40000,
                SubTierEnum.Tier3 => 60000,
                _ => 0
            };
            await twitchHelperService.AddPointsToUser(giftSubParams.BroadcasterChannelId, giftSubParams.ChatterChannelId, pointsToAdd, giftSubParams.BroadcasterChannelName, giftSubParams.ChatterChannelName);
            await twitchHelperService.SendTwitchMessageToChannel(giftSubParams.BroadcasterChannelId, giftSubParams.BroadcasterChannelName, $"Thank you to {giftSubParams.ChatterChannelName} for gifting {(giftSubParams.Total == 1 ? "a sub" : $"{giftSubParams.Total} subs")}! They have gifted {giftSubParams.CumulativeTotal} subs in total!");

            await AddSubathonTime(x => x.AddSubTime(giftSubParams.BroadcasterChannelId, giftSubParams.ChatterChannelId, giftSubParams.SubTier, giftSubParams.Total));

            if (!giftSubParams.IsAnonymous)
            {
                await UpdateMonthlyTotals(giftSubParams.BroadcasterChannelId, giftSubParams.ChatterChannelId, bitsDonated: 0, subsGifted: giftSubParams.Total);
            }
        }

        public async Task HandleChannelSubEvent(ChannelSubscribeParams subParams)
        {
            streamStatsService.UpdateStreamStat(subParams.BroadcasterChannelId, StreamStatType.NewSubscribers);

            if (subParams.IsGift)
            {
                Log.Information("Subscription is a gift, skipping points addition.");
                return;
            }

            var pointsToAdd = subParams.SubTier switch
            {
                SubTierEnum.Tier1 => 20000,
                SubTierEnum.Tier2 => 40000,
                SubTierEnum.Tier3 => 60000,
                _ => 0
            };

            await twitchHelperService.AddPointsToUser(subParams.BroadcasterChannelId, subParams.ChatterChannelId, pointsToAdd, subParams.BroadcasterChannelName, subParams.ChatterChannelName);
            await twitchHelperService.SendTwitchMessageToChannel(subParams.BroadcasterChannelId, subParams.BroadcasterChannelName, $"Thank you for the subscription, {subParams.ChatterChannelName}! <3");
        }

        public async Task HandlePredictionEndEvent(ChannelPredictionEndParams channelPredictionEndParams)
        {
            if (channelPredictionEndParams.PredictionStatus.ToLower() != "resolved")
            {
                Log.Information("Prediction is not resolved, skipping message.");
                return;
            }

            var wonOutcome = channelPredictionEndParams.WonOutcome;
            var winners = wonOutcome.TopPredictors.Select(x => $"{x.UserName} ({x.ChannelPointsWon})").ToList();
            var winnersString = string.Join(", ", winners);

            await twitchHelperService.SendTwitchMessageToChannel(channelPredictionEndParams.BroadcasterChannelId, channelPredictionEndParams.BroadcasterChannelName, $"The prediction has ended! The winning outcome was: {wonOutcome.Title}. The winners are: {winnersString}. They won {wonOutcome.ChannelPoints} channel points!");
        }

        public async Task HandlePredictionLockedEvent(ChannelPredictionLockedParams channelPredictionLockedParams)
        {
            await twitchHelperService.SendTwitchMessageToChannel(channelPredictionLockedParams.BroadcasterChannelId, channelPredictionLockedParams.BroadcasterChannelName, $"The prediction has been locked! Good luck :)");
        }

        public async Task HandlePredictionBeginEvent(ChannelPredictionBeginParams channelPredictionBeginParams)
        {
            var outcomes = channelPredictionBeginParams.PredictionOutcomeOptions.Select(x => x.Title).ToList();
            var outcomesString = string.Join(", ", outcomes);
            await twitchHelperService.SendAnnouncementMessageToChannel(channelPredictionBeginParams.BroadcasterChannelId, channelPredictionBeginParams.BroadcasterChannelName, $"A new prediction has started! The possible outcomes are: {outcomesString}. Good luck :)");
        }

        public async Task HandlePollBeginEvent(ChannelPollBeginParams channelPollBeginParams)
        {
            var options = channelPollBeginParams.PollChoices.Select(x => x.Title).ToList();
            var optionsString = string.Join(", ", options);

            await twitchHelperService.SendAnnouncementMessageToChannel(channelPollBeginParams.BroadcasterChannelId, channelPollBeginParams.BroadcasterChannelName, $"A new poll has started! The possible options are: {optionsString}. Vote wisely :)");
        }

        public async Task HandlePollEndEvent(ChannelPollEndParams channelPollEndParams)
        {
            var winners = channelPollEndParams.PollEndResults
                .Where(x => x.Votes == channelPollEndParams.PollEndResults.Max(y => y.Votes))
                .Select(x => x.Title)
                .ToList();

            var winnersString = string.Join(", ", winners);
            await twitchHelperService.SendTwitchMessageToChannel(channelPollEndParams.BroadcasterChannelId, channelPollEndParams.BroadcasterChannelName, $"The poll has ended! The winning options are: {winnersString}. Thank you for voting!");
        }

        public async Task HandleRaidEvent(ChannelRaidParams raidParams)
        {
            await twitchHelperService.SendTwitchMessageToChannel(raidParams.BroadcasterChannelId, raidParams.BroadcasterChannelName, $"Welcome {raidParams.RaidingChannelName} and their {raidParams.Viewers} viewers! Thank you for the raid! <3 Make sure to check out their channel at https://twitch.tv/{raidParams.RaidingChannelName.ToLower()}");

            // the minimum is per channel so a channel can decide what counts as a real raid
            // rather than a troll one, or shout out every raid by setting it to zero
            var minimumViewers = configHelperService.GetAutoShoutoutMinimumViewers(raidParams.BroadcasterChannelId);

            if (raidParams.Viewers >= minimumViewers)
            {
                var channel = twitchApiConnection.GetBotApiClient();

                if (channel != null)
                {
                    try
                    {
                        await twitchApiInteractionService.ShoutoutChannel(channel.ApiClient, raidParams.BroadcasterChannelId, raidParams.RaidingChannelId, channel.TwitchChannelClientId);
                        Log.Information($"[Raid] Shouted out {raidParams.RaidingChannelName} after a {raidParams.Viewers} viewer raid");
                    }
                    catch (Exception ex)
                    {
                        // twitch rate limits shoutouts, so a failure here must not take down
                        // the rest of the raid handling
                        Log.Warning(ex, $"[Raid] Could not shout out {raidParams.RaidingChannelName}");
                    }
                }
            }
        }

        public async Task HandleChannelPointsRedeemedEvent(ChannelPointsRedeemedParams redeemedParams)
        {
            Log.Information($"[Channel Points] {redeemedParams.ChatterChannelName} redeemed {redeemedParams.RewardTitle} ({redeemedParams.RewardCost}) in {redeemedParams.BroadcasterChannelName}");

            // the broadcaster or a mod has already dealt with this one manually
            if (string.Equals(redeemedParams.RedemptionStatus, "ACTION_TAKEN", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var reward = await context.ChannelPointRewards
                    .FirstOrDefaultAsync(x =>
                        x.Channel.BroadcasterTwitchChannelId == redeemedParams.BroadcasterChannelId &&
                        x.RewardTitle.ToLower() == redeemedParams.RewardTitle.ToLower());

                if (reward == null || !reward.Enabled)
                {
                    return;
                }

                reward.TimesRedeemed++;
                await context.SaveChangesAsync();

                var message = reward.ResponseMessage.Replace("{user}", redeemedParams.ChatterChannelName);
                await twitchHelperService.SendTwitchMessageToChannel(redeemedParams.BroadcasterChannelId, redeemedParams.BroadcasterChannelName, message);
            }
        }

        /// <summary>
        /// Runs a subathon update in its own scope. Subathon time must never take down the event
        /// that triggered it, so failures are logged rather than thrown.
        /// </summary>
        private async Task AddSubathonTime(Func<ISubathonDataService, Task> action)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var subathonDataService = scope.ServiceProvider.GetRequiredService<ISubathonDataService>();
                await action(subathonDataService);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Subathon] Error adding subathon time");
            }
        }

        /// <summary>
        /// Keeps the monthly bits and gifted sub totals that drive the monthly leaderboard roles.
        /// These columns existed but nothing was writing to them.
        /// </summary>
        private async Task UpdateMonthlyTotals(string broadcasterChannelId, string chatterChannelId, long bitsDonated, int subsGifted)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var stats = await context.ChannelUserStats
                    .FirstOrDefaultAsync(x => x.User.TwitchUserId == chatterChannelId && x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId);

                if (stats == null)
                {
                    return;
                }

                stats.BitsDonatedThisMonth += (int)bitsDonated;
                stats.GiftedSubsThisMonth += subsGifted;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Monthly Leaderboards] Error updating the monthly totals");
            }
        }

        /// <summary>
        /// How long after going live the daily points open, so they're for people actually watching
        /// </summary>
        public static readonly TimeSpan DailyPointsDelay = TimeSpan.FromMinutes(30);

        /// <summary>
        /// A stream that comes back within this long of ending is the same broadcast dropping out
        /// and reconnecting, not a new one
        /// </summary>
        public static readonly TimeSpan ReconnectWindow = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Twitch's api can keep showing a stream as live for a little while after it ends, and
        /// not show it yet just after it starts, so the checks against it allow for this long
        /// </summary>
        public static readonly TimeSpan ApiLag = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Twitch has to say a stream isn't live this many checks in a row before the bot ends it,
        /// so one failed call doesn't end a stream
        /// </summary>
        public const int OfflineChecksNeeded = 3;

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _streamLocks = new();
        private readonly ConcurrentDictionary<string, int> _offlineChecks = new();

        /// <summary>
        /// The twitch event, startup and the regular check can all arrive together, so a channel's
        /// stream changes happen one at a time
        /// </summary>
        private async Task<IDisposable> LockStream(string broadcasterId)
        {
            var streamLock = _streamLocks.GetOrAdd(broadcasterId, _ => new SemaphoreSlim(1, 1));
            await streamLock.WaitAsync();
            return new StreamLockRelease(streamLock);
        }

        private sealed class StreamLockRelease(SemaphoreSlim streamLock) : IDisposable
        {
            public void Dispose() => streamLock.Release();
        }

        public async Task HandleStreamOnline(string broadcasterId, string broadcasterName, string twitchStreamId, DateTime startedAt)
        {
            using var streamLock = await LockStream(broadcasterId);
            _offlineChecks.TryRemove(broadcasterId, out _);

            var state = configHelperService.GetStreamState(broadcasterId);

            if (state.Live && state.TwitchStreamId == null)
            {
                // live from before stream ids were stored, e.g. the update adding them being deployed
                // mid stream. It's the stream the bot already knew about, so it just takes the id
                Log.Information($"[Stream] {broadcasterName} was live before stream ids were stored, taking twitch stream {twitchStreamId} as the current one");
                await configHelperService.MarkStreamLive(broadcasterId, twitchStreamId, null);
                await ResumeStream(broadcasterId, broadcasterName, state);
                return;
            }

            if (state.TwitchStreamId == twitchStreamId)
            {
                if (state.Live)
                {
                    // the bot restarting mid stream, or hearing about the same stream twice
                    await ResumeStream(broadcasterId, broadcasterName, state);
                    return;
                }

                if (DateTime.UtcNow - state.LastStreamEnd < ApiLag)
                {
                    // twitch's api still showing the broadcast that just ended
                    return;
                }

                // it was ended by mistake, most likely twitch's api failing for a few checks, and it never stopped
                Log.Warning($"[Stream] {broadcasterName} was marked offline but twitch stream {twitchStreamId} is still going, carrying it on");
                await ContinueBroadcast(broadcasterId, broadcasterName, twitchStreamId, state);
                return;
            }

            if (state.Live)
            {
                // a new broadcast while the old one is still marked live means the bot missed it ending.
                // There's no telling how long the gap was, so it counts as a new broadcast. Daily points
                // won't reset streaks twice in a day either way
                Log.Warning($"[Stream] {broadcasterName} went live on a new broadcast before the bot saw the last one end");
                await streamStatsService.EndStream(broadcasterId);
                await StartNewBroadcast(broadcasterId, broadcasterName, twitchStreamId, startedAt);
                return;
            }

            if (startedAt - state.LastStreamEnd <= ReconnectWindow)
            {
                await ContinueBroadcast(broadcasterId, broadcasterName, twitchStreamId, state);
                return;
            }

            await StartNewBroadcast(broadcasterId, broadcasterName, twitchStreamId, startedAt);
        }

        public async Task HandleStreamOffline(string broadcasterId, string broadcasterName)
        {
            using var streamLock = await LockStream(broadcasterId);
            _offlineChecks.TryRemove(broadcasterId, out _);

            var state = configHelperService.GetStreamState(broadcasterId);

            if (!state.Live)
            {
                Log.Information($"[Stream] {broadcasterName} is already offline");
                return;
            }

            Log.Information($"[Stream] {broadcasterName} has gone offline");

            // daily point claims are kept, so if this is the stream dropping out the claims carry on
            // when it comes back
            await configHelperService.UpdateStreamLiveStatus(broadcasterId, false);
            await configHelperService.UpdateDailyPointsStatus(broadcasterId, false);
            twitchHelperService.ClearStreamChattersList(broadcasterId);
            await streamStatsService.EndStream(broadcasterId);
        }

        public async Task CheckStreamStatus(string broadcasterId, string broadcasterName, GetStreamsResponse? liveStream)
        {
            if (liveStream != null)
            {
                await HandleStreamOnline(broadcasterId, broadcasterName, liveStream.Id, liveStream.StartedAt);
                return;
            }

            var state = configHelperService.GetStreamState(broadcasterId);

            if (!state.Live)
            {
                _offlineChecks.TryRemove(broadcasterId, out _);
                return;
            }

            // twitch's api may not show a stream that's only just started
            if (DateTime.UtcNow - state.LastStreamStart < ApiLag)
            {
                return;
            }

            var offlineChecks = _offlineChecks.AddOrUpdate(broadcasterId, 1, (_, checks) => checks + 1);

            if (offlineChecks < OfflineChecksNeeded)
            {
                Log.Information($"[Stream] {broadcasterName} is marked live but twitch says it isn't ({offlineChecks}/{OfflineChecksNeeded})");
                return;
            }

            // the stream ended without the bot hearing about it, e.g. it was down or disconnected
            Log.Warning($"[Stream] {broadcasterName} ended without the bot hearing about it, ending it now");
            await HandleStreamOffline(broadcasterId, broadcasterName);
        }

        /// <summary>
        /// The bot already knew about this broadcast, so nothing about going live is redone
        /// </summary>
        private async Task ResumeStream(string broadcasterId, string broadcasterName, StreamState state)
        {
            if (state.DailyPointsAllowed)
            {
                return;
            }

            Log.Information($"[Stream] {broadcasterName} is still on the same broadcast, making sure the daily points open");
            await OpenDailyPointsOnTime(broadcasterId, broadcasterName, state.LastStreamStart);
        }

        /// <summary>
        /// The stream dropped and came back, so it carries on the same broadcast: no second
        /// announcement, stream minutes, stats or boss countdown
        /// </summary>
        private async Task ContinueBroadcast(string broadcasterId, string broadcasterName, string twitchStreamId, StreamState state)
        {
            Log.Information($"[Stream] {broadcasterName} is back, carrying on the broadcast that started at {state.LastStreamStart}");

            await configHelperService.MarkStreamLive(broadcasterId, twitchStreamId, null);
            await OpenDailyPointsOnTime(broadcasterId, broadcasterName, state.LastStreamStart);
        }

        private async Task StartNewBroadcast(string broadcasterId, string broadcasterName, string twitchStreamId, DateTime startedAt)
        {
            Log.Information($"[Stream] {broadcasterName} has gone live on a new broadcast, started at {startedAt}");

            await configHelperService.MarkStreamLive(broadcasterId, twitchStreamId, startedAt);

            // closed until they open for this stream, in case the bot missed the last stream ending
            await configHelperService.UpdateDailyPointsStatus(broadcasterId, false);

            twitchHelperService.ClearStreamChattersList(broadcasterId);
            await streamStatsService.StartNewStream(broadcasterId);

            if (configHelperService.IsDiscordEnabled(broadcasterId))
            {
                var discordConfig = configHelperService.GetDiscordConfig(broadcasterId);
                if (discordConfig != null && discordConfig.DiscordStreamAnnouncementChannelId != null)
                {
                    await discordHelperService.SendMessage(discordConfig.DiscordStreamAnnouncementChannelId.Value, $"Hey @everyone !!! {broadcasterName} is now live!! Woooo! Tune in at https://twitch.tv/{broadcasterName.ToLower()}");
                }
            }

            using (var scope = serviceProvider.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IHoursDataService>().ResetStreamMinutesForBroadcaster(broadcasterId);
            }

            await OpenDailyPointsOnTime(broadcasterId, broadcasterName, startedAt);

            // counted from the stream starting, in case the bot only noticed it late
            var bossIn = startedAt + TimeSpan.FromMinutes(45) - DateTime.UtcNow;
            BackgroundJob.Schedule<ITwitchBossesDataService>(svc =>
                svc.StartBossFightCountdown(
                    broadcasterId,
                    broadcasterName,
                    null),
                bossIn > TimeSpan.FromMinutes(1) ? bossIn : TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Opens the daily points 30 minutes after the broadcast started, straight away if that's passed.
        /// If they've already been opened today there's nothing to wait for, as they won't reset streaks
        /// again and anyone who already claimed stays claimed.
        /// Opening them twice does nothing, so a job left over from before a restart is harmless.
        /// </summary>
        private async Task OpenDailyPointsOnTime(string broadcasterId, string broadcasterName, DateTime broadcastStarted)
        {
            var state = configHelperService.GetStreamState(broadcasterId);
            var opensIn = broadcastStarted + DailyPointsDelay - DateTime.UtcNow;

            if (opensIn <= TimeSpan.Zero || state.LastDailyPointsAllowed.Date == DateTime.UtcNow.Date)
            {
                using var scope = serviceProvider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IDailyPointsDataService>().AllowDailyPointsCollecting(broadcasterId);
                return;
            }

            BackgroundJob.Schedule<IDailyPointsDataService>(svc => svc.AllowDailyPointsCollecting(broadcasterId), opensIn);
            Log.Information($"[Stream] Daily points for {broadcasterName} will open in {opensIn.TotalMinutes:N0} minutes");
        }
    }
}
