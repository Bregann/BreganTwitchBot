using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Events
{
    public class Subscribers
    {
        public static async Task HandleNewSubscriberEvent(TwitchLib.Client.Enums.SubscriptionPlan subType, string username, string userId)
        {
            var subName = "";

            switch (subType)
            {
                case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                    subName = "tier 1 subscription PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 20000);
                    StreamStatsService.UpdateStreamStat(20000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                    subName = "TWITCH PRIME SUBSCRIPTION!!!! ANY PRIMERS? <3 <3 PogChamp blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 30000);
                    StreamStatsService.UpdateStreamStat(30000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                    subName = "tier 2 subscription PogChamp PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 40000);
                    StreamStatsService.UpdateStreamStat(40000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                    subName = "tier 3 subscription PogChamp PogChamp PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksJ0F blocksWOTG blocksGuineaG blocksEcho blocksEcho";
                    await PointsHelper.AddUserPoints(userId, 100000);
                    StreamStatsService.UpdateStreamStat(100000, StatTypes.PointsGainedSubscribing);
                    break;
            }

            StreamStatsService.UpdateStreamStat(1, StatTypes.NewSubscriber);
            TwitchHelper.SendMessage($"Welcome {username}to the {AppConfig.BroadcasterName} squad with a {subName}");
        }

    }
}
