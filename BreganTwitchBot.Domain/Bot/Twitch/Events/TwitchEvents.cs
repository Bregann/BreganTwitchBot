using BreganTwitchBot.Core;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Commands;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DiceRoll;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.WordBlacklist;
using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using BreganTwitchBot.Domain.Bot.Twitch.Services;
using BreganTwitchBot.Domain.Bot.Twitch.Services.Stats;
using BreganTwitchBot.Domain.Bot.Twitch.Services.Stats.Enums;
using Serilog;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;
using BreganTwitchBot.Infrastructure.Database.Models;

namespace BreganTwitchBot.Domain.Bot.Twitch.Events
{
    public class TwitchEvents
    {
        public static void SetupTwitchEvents(bool pubsubRefresh = false)
        {
            if (pubsubRefresh)
            {
                TwitchPubSubConnection.PubSubClient.OnBitsReceivedV2 += BitsReceived;
                TwitchPubSubConnection.PubSubClient.OnFollow += UserFollowed;
                TwitchPubSubConnection.PubSubClient.OnChannelPointsRewardRedeemed += ChannelPointsRewardRedeemed;
            }
            else
            {
                TwitchBotConnection.Client.OnChatCommandReceived += ChatCommandReceived;
                TwitchBotConnection.Client.OnMessageReceived += MessageReceived;
                TwitchBotConnection.Client.OnUserBanned += UserBanned;
                TwitchBotConnection.Client.OnUserTimedout += UserTimedout;
                TwitchBotConnection.Client.OnMessageSent += MessageSent;
                TwitchBotConnection.Client.OnGiftedSubscription += GiftedSubscription;
                TwitchBotConnection.Client.OnNewSubscriber += NewSubscriber;
                TwitchBotConnection.Client.OnReSubscriber += ReSubscriber;
                TwitchBotConnection.Client.OnRaidNotification += RaidNotification;
                TwitchBotConnection.Client.OnUserJoined += UserJoined;
                TwitchBotConnection.Client.OnUserLeft += UserLeft;
                TwitchPubSubConnection.PubSubClient.OnBitsReceivedV2 += BitsReceived;
                TwitchPubSubConnection.PubSubClient.OnFollow += UserFollowed;
                TwitchPubSubConnection.PubSubClient.OnChannelPointsRewardRedeemed += ChannelPointsRewardRedeemed;
            }
        }

        private static void ChannelPointsRewardRedeemed(object? sender, OnChannelPointsRewardRedeemedArgs e)
        {
            try
            {
                Log.Information($"[Channel Reward Redeemed] Name: {e.RewardRedeemed.Redemption.Reward.Title} Cost: {e.RewardRedeemed.Redemption.Reward.Cost} User who redeemed: {e.RewardRedeemed.Redemption.User.DisplayName}");
                StreamStatsService.UpdateStreamStat(1, StatTypes.AmountOfRewardsRedeemd);
                StreamStatsService.UpdateStreamStat(e.RewardRedeemed.Redemption.Reward.Cost, StatTypes.RewardRedeemCost);

                if (e.RewardRedeemed.Redemption.Status == "ACTION_TAKEN")
                {
                    return;
                }

                switch (e.RewardRedeemed.Redemption.Reward.Title.ToLower())
                {
                    case "goose":
                        TwitchHelper.SendMessage($"{e.RewardRedeemed.Redemption.User.DisplayName} has redeemed Goose! Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose");
                        break;

                    case "dice roll":
                        DiceRoll.AddNormalDiceRoll(e.RewardRedeemed.Redemption.User.DisplayName.ToLower());
                        TwitchHelper.SendMessage($"{e.RewardRedeemed.Redemption.User.DisplayName} has redeemed Dice Roll! Do your roll with !roll");
                        break;
                }
            }
            catch (Exception xe)
            {
                Log.Information($"[channel points error] {xe}");
                return;
            }
        }

        private static void UserFollowed(object? sender, OnFollowArgs e)
        {
            Log.Information($"[New Twitch Follow] {e.DisplayName} just followed! Twitch ID: {e.UserId}");
        }

        private static void BitsReceived(object? sender, OnBitsReceivedV2Args e)
        {
            try
            {
                Commands.Modules.Subathon.Subathon.AddSubathonBitsTime(e.BitsUsed, e.UserName.ToLower());

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.TwitchUserId == e.UserId).FirstOrDefault();

                    if (user != null)
                    {
                        user.BitsDonatedThisMonth += e.BitsUsed;
                        context.SaveChanges();
                    }
                }

                if (e.BitsUsed <= 4)
                {
                    Log.Information($"[PubSub] Just received {e.BitsUsed} bits from {e.UserName}. That brings their total to {e.TotalBitsUsed} bits!");
                    return;
                }

                TwitchHelper.SendMessage($"{e.UserName} has donated {e.BitsUsed:N0} bits with a grand total of {e.TotalBitsUsed:N0} donated PogChamp");
                Log.Information($"[PubSub] Just received {e.BitsUsed} bits from {e.UserName}. That brings their total to {e.TotalBitsUsed} bits!");
            }
            catch (Exception xe)
            {
                Log.Information($"[bits error] {xe}");
                return;
            }
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
            var subName = "";

            Commands.Modules.Subathon.Subathon.AddSubathonSubTime(e.Subscriber.SubscriptionPlan, e.Subscriber.DisplayName.ToLower());

            switch (e.Subscriber.SubscriptionPlan)
            {
                case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                    subName = "tier 1 subscription PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho blocksEcho";
                    PointsHelper.AddUserPoints(e.Subscriber.DisplayName.ToLower(), 20000);
                    StreamStatsService.UpdateStreamStat(20000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                    subName = "TWITCH PRIME SUBSCRIPTION!!!! ANY PRIMERS? <3 <3 PogChamp blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksEcho blocksEcho";
                    PointsHelper.AddUserPoints(e.Subscriber.DisplayName.ToLower(), 30000);
                    StreamStatsService.UpdateStreamStat(30000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                    subName = "tier 2 subscription PogChamp PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksEcho blocksEcho";
                    PointsHelper.AddUserPoints(e.Subscriber.DisplayName.ToLower(), 40000);
                    StreamStatsService.UpdateStreamStat(40000, StatTypes.PointsGainedSubscribing);
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                    subName = "tier 3 subscription PogChamp PogChamp PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksJ0F blocksWOTG blocksGuineaG blocksEcho blocksEcho";
                    PointsHelper.AddUserPoints(e.Subscriber.DisplayName.ToLower(), 100000);
                    StreamStatsService.UpdateStreamStat(100000, StatTypes.PointsGainedSubscribing);
                    break;
            }

            StreamStatsService.UpdateStreamStat(1, StatTypes.NewSubscriber);
            TwitchHelper.SendMessage($"Welcome {e.Subscriber.DisplayName}to the {AppConfig.BroadcasterName} squad with a {subName}");
        }

        private static void GiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
        {
            var subName = "";

            Commands.Modules.Subathon.Subathon.AddSubathonSubTime(e.GiftedSubscription.MsgParamSubPlan, e.GiftedSubscription.DisplayName.ToLower());

            switch (e.GiftedSubscription.MsgParamSubPlan)
            {
                case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                    PointsHelper.AddUserPoints(e.GiftedSubscription.DisplayName.ToLower(), 20000);
                    PointsHelper.AddUserPoints(e.GiftedSubscription.MsgParamRecipientUserName.ToLower(), 5000);
                    StreamStatsService.UpdateStreamStat(25000, StatTypes.PointsGainedSubscribing);
                    subName = "tier 1";
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                    PointsHelper.AddUserPoints(e.GiftedSubscription.DisplayName.ToLower(), 40000);
                    PointsHelper.AddUserPoints(e.GiftedSubscription.MsgParamRecipientUserName.ToLower(), 5000);
                    StreamStatsService.UpdateStreamStat(45000, StatTypes.PointsGainedSubscribing);
                    subName = "tier 2";
                    break;

                case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                    PointsHelper.AddUserPoints(e.GiftedSubscription.DisplayName.ToLower(), 100000);
                    PointsHelper.AddUserPoints(e.GiftedSubscription.MsgParamRecipientUserName.ToLower(), 5000);
                    StreamStatsService.UpdateStreamStat(105000, StatTypes.PointsGainedSubscribing);
                    subName = "tier 3";
                    break;

                default:
                    break;
            }

            try
            {
                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.Username == e.GiftedSubscription.DisplayName.ToLower()).FirstOrDefault();

                    if (user != null)
                    {
                        user.GiftedSubsThisMonth++;
                        context.SaveChanges();
                    }
                }

                Log.Information($"[Sub Leaderboard] +1 to {e.GiftedSubscription.DisplayName.ToLower()}");
            }
            catch (Exception eee)
            {
                Log.Fatal(eee.Message);
            }

            StreamStatsService.UpdateStreamStat(1, StatTypes.NewGiftedSubs);
            TwitchHelper.SendMessage($"Thank you {e.GiftedSubscription.DisplayName} for gifting {e.GiftedSubscription.MsgParamRecipientUserName} a {subName} subscription PogChamp <3 blocksGuinea1 blocksGuinea2 blocksGuinea3 blocksW blocksOK blocksGuinea blocksMarge blocksBitrate blocksSWIRL blocksFail blocksWOT blocksBANNED blocksJ0F blocksWOTG blocksGuineaG blocksEcho blocksEcho");
        }

        private static void MessageSent(object? sender, OnMessageSentArgs e)
        {
            Log.Information($"[Twitch Message Sent] {e.SentMessage.Message}");
        }

        private static void UserTimedout(object? sender, OnUserTimedoutArgs e)
        {
            StreamStatsService.UpdateStreamStat(1, StatTypes.TotalTimeouts);
            Log.Information($"[User Timed out in Stream] User banned: {e.UserTimeout.Username} Duration: {e.UserTimeout.TimeoutDuration} Reason: {e.UserTimeout.TimeoutReason}");
        }

        private static void UserBanned(object? sender, OnUserBannedArgs e)
        {
            StreamStatsService.UpdateStreamStat(1, StatTypes.TotalBans);
            Log.Information($"[User Banned in Stream] User banned: {e.UserBan.Username}");
        }

        private static void MessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            Log.Information($"[Twitch Message Received] Username: {e.ChatMessage.Username} Message: {e.ChatMessage.Message}");

            try
            {
                CommandHandler.HandleCustomCommand(e);

                WordBlacklist.OnMessageReceived(e);

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.TwitchUserId == e.ChatMessage.UserId).FirstOrDefault();
                    var uniqueUser = context.UniqueViewers.Where(x => x.Username == e.ChatMessage.Username).FirstOrDefault();

                    if (uniqueUser == null)
                    {
                        context.UniqueViewers.Add(new UniqueViewers
                        {
                            Username = e.ChatMessage.Username
                        });
                    }

                    if (user != null)
                    {
                        user.TotalMessages++;
                        user.Username = e.ChatMessage.Username.ToLower();
                        user.InStream = true;
                        user.IsSub = e.ChatMessage.IsSubscriber;
                    }
                    else
                    {
                        var newUser = new Users
                        {
                            TwitchUserId = e.ChatMessage.UserId,
                            Username = e.ChatMessage.Username.ToLower(),
                            InStream = true,
                            IsSub = e.ChatMessage.IsSubscriber,
                            MinutesInStream = 0,
                            Points = 0,
                            IsSuperMod = false,
                            TotalMessages = 1,
                            DiscordUserId = 0,
                            LastSeenDate = DateTime.UtcNow,
                            PointsGambled = 0,
                            PointsWon = 0,
                            PointsLost = 0,
                            TotalSpins = 0,
                            Tier1Wins = 0,
                            Tier2Wins = 0,
                            Tier3Wins = 0,
                            JackpotWins = 0,
                            SmorcWins = 0,
                            CurrentStreak = 0,
                            HighestStreak = 0,
                            TotalTimesClaimed = 0,
                            TotalPointsClaimed = 0,
                            PointsLastClaimed = new DateTime(0),
                            PointsClaimedThisStream = false,
                            GiftedSubsThisMonth = 0,
                            BitsDonatedThisMonth = 0,
                            MarblesWins = 0,
                            DiceRolls = 0,
                            BonusDiceRolls = 0,
                            DiscordDailyStreak = 0,
                            DiscordDailyTotalClaims = 0,
                            DiscordDailyClaimed = false,
                            DiscordLevel = 0,
                            DiscordXp = 0,
                            DiscordLevelUpNotifsEnabled = true,
                            PrestigeLevel = 0,
                            MinutesWatchedThisStream = 0,
                            MinutesWatchedThisWeek = 0,
                            MinutesWatchedThisMonth = 0,
                            BossesDone = 0,
                            BossesPointsWon = 0,
                            TimeoutStrikes = 0,
                            WarnStrikes = 0
                        };

                        context.Users.Add(newUser);
                    }

                    context.SaveChanges();
                }

                StreamStatsService.UpdateStreamStat(1, StatTypes.MessagesReceived);
            }
            catch (Exception ex)
            {
                Log.Information($"[Commands] {ex}");
            }
        }

        private static async void ChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            try
            {
                await CommandHandler.HandleCommand(e);
            }
            catch (Exception ex)
            {
                Log.Information($"[Twitch Commands] {ex}");
            }
        }
    }
}