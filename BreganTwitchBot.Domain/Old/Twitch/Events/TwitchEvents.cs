using BreganTwitchBot.Core;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Commands;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DiceRoll;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.WordBlacklist;
using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using BreganTwitchBot.Domain.Bot.Twitch.Services;
using Serilog;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;
using BreganTwitchBot.Infrastructure.Database.Models;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.WordBlacklist;

namespace BreganTwitchBot.Domain.Bot.Twitch.Events
{
    public class TwitchEvents
    {
        public static void SetupTwitchEvents(bool pubsubRefresh = false)
        {
            TwitchBotConnection.Client.OnNewSubscriber += NewSubscriber;
            TwitchBotConnection.Client.OnReSubscriber += ReSubscriber;
            TwitchBotConnection.Client.OnRaidNotification += RaidNotification;
            TwitchBotConnection.Client.OnUserJoined += UserJoined;
            TwitchBotConnection.Client.OnUserLeft += UserLeft;
        }



        private static void UserLeft(object? sender, OnUserLeftArgs e)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == e.Username).FirstOrDefault();

                if (user != null)
                {
                    user.InStream = false;
                    context.SaveChanges();
                }
            }

            Log.Information($"[User Left] {e.Username} left the stream");
        }

        private static void UserJoined(object? sender, OnUserJoinedArgs e)
        {
            Log.Information($"[User Joined] {e.Username} joined the stream");
        }

        private static async void RaidNotification(object? sender, OnRaidNotificationArgs e)
        {
            int.TryParse(e.RaidNotification.MsgParamViewerCount, out var raidAmount);

            if (raidAmount > 50)
            {
                HangfireJobs.StartRaidFollowersOffJob();
            }

            TwitchHelper.SendMessage($"Welcome {e.RaidNotification.DisplayName} and their humble {e.RaidNotification.MsgParamViewerCount} raiders! Make sure to check out {e.RaidNotification.DisplayName} at twitch.tv/{e.RaidNotification.DisplayName} !");
        }

        private static void ReSubscriber(object? sender, OnReSubscriberArgs e)
        {
            var subName = "";
            int.TryParse(e.ReSubscriber.MsgParamCumulativeMonths, out var months);
            int.TryParse(e.ReSubscriber.MsgParamStreakMonths, out var streakMonths);

            Commands.Modules.Subathon.Subathon.AddSubathonSubTime(e.ReSubscriber.SubscriptionPlan, e.ReSubscriber.DisplayName.ToLower());

            var sharedStreakMessage = streakMonths == 0 ? "they did not share their sub streak :(" : $"They are on a {streakMonths} month sub streak <3 PogChamp blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho blocksEcho";

            switch (e.ReSubscriber.SubscriptionPlan)
            {
                case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                    subName = "tier 1";
                    PointsHelper.AddUserPoints(e.ReSubscriber.DisplayName.ToLower(), 20000 + months * 2000);
                    StreamStatsService.UpdateStreamStat(20000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                    subName = "TWITCH PRIME!!! ANY PRIMERS?";
                    PointsHelper.AddUserPoints(e.ReSubscriber.DisplayName.ToLower(), 30000 + months * 2000);
                    StreamStatsService.UpdateStreamStat(30000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                    subName = "tier 2";
                    PointsHelper.AddUserPoints(e.ReSubscriber.DisplayName.ToLower(), 40000 + months * 2000);
                    StreamStatsService.UpdateStreamStat(40000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                    subName = "tier 3";
                    PointsHelper.AddUserPoints(e.ReSubscriber.DisplayName.ToLower(), 100000 + months * 2000);
                    StreamStatsService.UpdateStreamStat(100000 + months * 2000, StatTypes.PointsGainedSubscribing);
                    break;
            }

            TwitchHelper.SendMessage($"Welcome back {e.ReSubscriber.DisplayName} for {months} months with a {subName} subscription! {sharedStreakMessage}");
        }

        private static void NewSubscriber(object? sender, OnNewSubscriberArgs e)
        {

        }








    }
}