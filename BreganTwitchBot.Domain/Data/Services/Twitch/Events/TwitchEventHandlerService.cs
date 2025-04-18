using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Events;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Events
{
    public class TwitchEventHandlerService(ITwitchHelperService twitchHelperService, ITwitchApiInteractionService twitchApiInteractionService, ITwitchApiConnection twitchApiConnection) : ITwitchEventHandlerService
    {
        public async Task HandleChannelCheerEvent(BitsCheeredParams cheerParams)
        {
            await twitchHelperService.SendTwitchMessageToChannel(cheerParams.BroadcasterChannelId, cheerParams.BroadcasterChannelName, $"Thank you for the {cheerParams.Amount} bits, {cheerParams.ChatterChannelName}! PogChamp");
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
            var pointsToAdd = giftSubParams.SubTier switch
            {
                SubTierEnum.Tier1 => 20000,
                SubTierEnum.Tier2 => 40000,
                SubTierEnum.Tier3 => 60000,
                _ => 0
            };
            await twitchHelperService.AddPointsToUser(giftSubParams.BroadcasterChannelId, giftSubParams.ChatterChannelId, pointsToAdd, giftSubParams.BroadcasterChannelName, giftSubParams.ChatterChannelName);
            await twitchHelperService.SendTwitchMessageToChannel(giftSubParams.BroadcasterChannelId, giftSubParams.BroadcasterChannelName, $"Thank you to {giftSubParams.ChatterChannelName} for gifting {(giftSubParams.Total == 1 ? "a sub" : $"{giftSubParams.Total} subs")}! They have gifted {giftSubParams.CumulativeTotal} subs in total!");
        }

        public async Task HandleChannelSubEvent(ChannelSubscribeParams subParams)
        {
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

            // if their viewers are greater than 5 we can give them a shoutout as it's probably not a troll raid
            if (raidParams.Viewers > 5)
            {
                var channel = twitchApiConnection.GetBotTwitchApiClientFromBroadcasterChannelId(raidParams.BroadcasterChannelId);

                if (channel != null)
                {
                    await twitchApiInteractionService.ShoutoutChannel(channel.ApiClient, raidParams.BroadcasterChannelId, raidParams.RaidingChannelId, channel.TwitchChannelClientId);
                }
            }
        }
    }
}
