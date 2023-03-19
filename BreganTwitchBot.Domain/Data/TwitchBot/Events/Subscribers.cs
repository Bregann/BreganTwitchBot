using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Events
{
    public class Subscribers
    {
        public static async Task HandleGiftSubscriptionEvent(TwitchLib.Client.Enums.SubscriptionPlan subType, string gifterUsername, string recipientUsername, string gifterUserId, string recipientUserId)
        {
            try
            {
                var subName = "";

                switch (subType)
                {
                    case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                    case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                    case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                        await PointsHelper.AddUserPoints(gifterUserId, 20000);
                        await PointsHelper.AddUserPoints(recipientUserId, 5000);
                        await StreamStatsService.UpdateStreamStat(25000, StatTypes.PointsGainedSubscribing);
                        subName = "tier 1";
                        break;

                    case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                        await PointsHelper.AddUserPoints(gifterUserId, 40000);
                        await PointsHelper.AddUserPoints(recipientUserId, 5000);
                        await StreamStatsService.UpdateStreamStat(45000, StatTypes.PointsGainedSubscribing);
                        subName = "tier 2";
                        break;

                    case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                        await PointsHelper.AddUserPoints(gifterUserId, 100000);
                        await PointsHelper.AddUserPoints(recipientUserId, 5000);
                        await StreamStatsService.UpdateStreamStat(105000, StatTypes.PointsGainedSubscribing);
                        subName = "tier 3";
                        break;
                    default:
                        break;
                }

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.TwitchUserId == gifterUserId).FirstOrDefault();

                    if (user != null)
                    {
                        user.GiftedSubsThisMonth++;
                        context.SaveChanges();
                    }
                }

                Log.Information($"[Sub Leaderboard] +1 to {gifterUsername}");

                await StreamStatsService.UpdateStreamStat(1, StatTypes.NewGiftedSubs);
                TwitchHelper.SendMessage($"Thank you {gifterUsername} for gifting {recipientUsername} a {subName} subscription PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksJ0F blocksWOTG blocksGuineaG blocksEcho");
            }

            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
            }
        }

        public static async Task HandleNewSubscriberEvent(TwitchLib.Client.Enums.SubscriptionPlan subType, string username, string userId)
        {
            var subName = "";

            switch (subType)
            {
                case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                    subName = "tier 1 subscription PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 20000);
                    await StreamStatsService.UpdateStreamStat(20000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                    subName = "TWITCH PRIME SUBSCRIPTION!!!! ANY PRIMERS? <3 <3 PogChamp blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 30000);
                    await StreamStatsService.UpdateStreamStat(30000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                    subName = "tier 2 subscription PogChamp PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 40000);
                    await StreamStatsService.UpdateStreamStat(40000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                    subName = "tier 3 subscription PogChamp PogChamp PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksJ0F blocksWOTG blocksGuineaG blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 100000);
                    await StreamStatsService.UpdateStreamStat(100000, StatTypes.PointsGainedSubscribing);
                    break;
            }

            await StreamStatsService.UpdateStreamStat(1, StatTypes.NewSubscriber);
            TwitchHelper.SendMessage($"Welcome {username}to the {AppConfig.BroadcasterName} squad with a {subName}");
        }

        public static async Task HandleResubscriberEvent(TwitchLib.Client.Enums.SubscriptionPlan subType, string username, string userId, string cumulativeMonthsSubscribed, string streakMonthsSubscribed)
        {
            var subName = "";
            int.TryParse(cumulativeMonthsSubscribed, out var months);
            int.TryParse(streakMonthsSubscribed, out var streakMonths);


            var sharedStreakMessage = streakMonths == 0 ? "they did not share their sub streak :(" : $"They are on a {streakMonths} month sub streak <3 PogChamp blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho";

            switch (subType)
            {
                case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                    subName = "tier 1";
                    await PointsHelper.AddUserPoints(userId, 20000 + months * 2000);
                    await StreamStatsService.UpdateStreamStat(20000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                    subName = "TWITCH PRIME!!! ANY PRIMERS?";
                    await PointsHelper.AddUserPoints(userId, 30000 + months * 2000);
                    await StreamStatsService.UpdateStreamStat(30000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                    subName = "tier 2";
                    await PointsHelper.AddUserPoints(userId, 40000 + months * 2000);
                    await StreamStatsService.UpdateStreamStat(40000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                    subName = "tier 3";
                    await PointsHelper.AddUserPoints(userId, 100000 + months * 2000);
                    await StreamStatsService.UpdateStreamStat(100000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;
            }

            TwitchHelper.SendMessage($"Welcome back {username} for {months} months with a {subName} subscription! {sharedStreakMessage}");
        }
    }
}
