using BreganTwitchBot.Domain.Database.Context;
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
        private static readonly TimeSpan DailyPointsDelay = TimeSpan.FromMinutes(30);

        public async Task HandleStreamLiveOnStartup(string broadcasterId, string broadcasterName, DateTime streamStartedAt)
        {
            var dailyPointsStatus = configHelperService.GetDailyPointsStatus(broadcasterId);
            var knownLive = await twitchHelperService.IsBroadcasterLive(broadcasterId);

            // the recorded start is when the bot handled going live, so it's at or after twitch's
            // start time for the same stream. An older one means a stream the bot missed, and the
            // leeway covers the clocks disagreeing a little
            var sameStream = knownLive && dailyPointsStatus.LastStreamDate >= streamStartedAt.AddMinutes(-5);

            if (!sameStream)
            {
                await HandleStreamOnline(broadcasterId, broadcasterName, DateTime.UtcNow - streamStartedAt > DailyPointsDelay);
                return;
            }

            // restarting mid stream mustn't redo going live: that wiped stream minutes, pinged
            // @everyone again, started a second boss countdown and opened daily points early
            Log.Information($"[Stream Startup] {broadcasterName} was already live before the bot restarted, carrying on the same stream");

            if (dailyPointsStatus.DailyPointsAllowed)
            {
                return;
            }

            // pick the daily points back up from when the stream went live, in case the job
            // scheduled before the restart has been lost. Opening them twice does nothing
            var opensIn = dailyPointsStatus.LastStreamDate + DailyPointsDelay - DateTime.UtcNow;

            if (opensIn <= TimeSpan.Zero)
            {
                using var scope = serviceProvider.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IDailyPointsDataService>().AllowDailyPointsCollecting(broadcasterId);
            }
            else
            {
                BackgroundJob.Schedule<IDailyPointsDataService>(svc => svc.AllowDailyPointsCollecting(broadcasterId), opensIn);
                Log.Information($"[Stream Startup] Daily points for {broadcasterName} rescheduled to open in {opensIn.TotalMinutes:N0} minutes");
            }
        }

        public async Task HandleStreamOnline(string broadcasterId, string broadcasterName, bool allowCollectionInstantly = false)
        {
            await configHelperService.UpdateStreamLiveStatus(broadcasterId, true);

            // closed until they open for this stream, in case the bot missed the last stream ending
            await configHelperService.UpdateDailyPointsStatus(broadcasterId, false);
            var discordEnabled = configHelperService.IsDiscordEnabled(broadcasterId);

            if (discordEnabled)
            {
                var discordConfig = configHelperService.GetDiscordConfig(broadcasterId);
                if (discordConfig != null && discordConfig.DiscordStreamAnnouncementChannelId != null)
                {
                    await discordHelperService.SendMessage(discordConfig.DiscordStreamAnnouncementChannelId.Value, $"Hey @everyone !!! {broadcasterName} is now live!! Woooo! Tune in at https://twitch.tv/{broadcasterName.ToLower()}");
                }
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var dailyPointsDataService = scope.ServiceProvider.GetRequiredService<IDailyPointsDataService>();
                var hoursDataService = scope.ServiceProvider.GetRequiredService<IHoursDataService>();

                if (allowCollectionInstantly)
                {
                    Log.Information($"Allowing daily points collection instantly for {broadcasterName}");
                    await dailyPointsDataService.AllowDailyPointsCollecting(broadcasterId);
                }
                else
                {
                    await hoursDataService.ResetStreamMinutesForBroadcaster(broadcasterId);
                    await dailyPointsDataService.ScheduleDailyPointsCollection(broadcasterId);
                }

                BackgroundJob.Schedule<ITwitchBossesDataService>(svc =>
                    svc.StartBossFightCountdown(
                        broadcasterId,
                        broadcasterName,
                        null),
                    TimeSpan.FromMinutes(45));
            }
        }
    }
}
