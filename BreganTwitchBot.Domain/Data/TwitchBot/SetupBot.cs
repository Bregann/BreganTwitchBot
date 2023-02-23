using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.WordBlacklist;
using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using BreganTwitchBot.Domain.Bot.Twitch.Services;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Events;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot.Stats;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;

namespace BreganTwitchBot.Domain.Data.TwitchBot
{
    public class SetupBot
    {
        public static async Task SetupTwitchBot()
        {
            var bot = new TwitchBotConnection();
            var twitchThread = new Thread(bot.Connect().GetAwaiter().GetResult);
            twitchThread.Start();
            Log.Information("[Twitch Client] Started Twitch Thread");

            await Task.Delay(2000);

            var twitchApi = new TwitchApiConnection();
            twitchApi.Connect();
            Log.Information("[Twitch API] Connected To Twitch API");

            await Task.Delay(2000);

            var pubSub = new TwitchPubSubConnection();
            pubSub.Connect();
            Log.Information("[Twitch PubSub] Connected To Twitch PubSub");

            await Task.Delay(2000);

            WordBlacklist.WordBlacklist.LoadBlacklistedWords();

            TwitchPubSubConnection.PubSubClient.OnChannelPointsRewardRedeemed += ChannelPointsRewardRedeemed;
            TwitchPubSubConnection.PubSubClient.OnBitsReceivedV2 += BitsReceived;
            TwitchPubSubConnection.PubSubClient.OnFollow += UserFollow;
            TwitchBotConnection.Client.OnChatCommandReceived += ChatCommandReceived;
            TwitchBotConnection.Client.OnMessageReceived += MessageReceived;
            TwitchBotConnection.Client.OnUserBanned += UserBanned;
            TwitchBotConnection.Client.OnUserTimedout += UserTimedOut;
            TwitchBotConnection.Client.OnMessageSent += MessageSent;
            TwitchBotConnection.Client.OnGiftedSubscription += GiftedSubscription;
            TwitchBotConnection.Client.OnNewSubscriber += NewSubscriber;
            TwitchBotConnection.Client.OnReSubscriber += ReSubscriber;
            TwitchBotConnection.Client.OnRaidNotification += RaidNotification;
            TwitchBotConnection.Client.OnUserJoined += UserJoined;
            TwitchBotConnection.Client.OnUserLeft += UserLeft;
        }

        private static async void ChannelPointsRewardRedeemed(object? sender, OnChannelPointsRewardRedeemedArgs e)
        {
            await ChannelPoints.HandleChannelPointsEvent(e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.Reward.Cost, e.RewardRedeemed.Redemption.User.DisplayName, e.RewardRedeemed.Redemption.Status);
        }

        private static void UserFollow(object? sender, OnFollowArgs e)
        {
            Log.Information($"[New Twitch Follow] {e.DisplayName} just followed! Twitch ID: {e.UserId}");
        }

        private static async void BitsReceived(object? sender, OnBitsReceivedV2Args e)
        {
            await Bits.HandleBitsEvent(e.UserId, e.UserName, e.BitsUsed, e.TotalBitsUsed);
        }

        private static async void ChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            try
            {
                await CommandHandler.HandleCommand(e);
            }
            catch (Exception ex)
            {
                Log.Warning($"[Twitch Commands] {ex}");
            }
        }

        private static async void MessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            Log.Information($"[Twitch Message Received] Username: {e.ChatMessage.Username} Message: {e.ChatMessage.Message}");

            try
            {
                await CommandHandler.HandleCustomCommand(e.ChatMessage.Message.Split(' ').FirstOrDefault(), e.ChatMessage.Username, e.ChatMessage.UserId);
                await WordBlacklist.WordBlacklist.HandleMessageChecks(e);
                await Message.HandleUserAddingOrUpdating(e.ChatMessage.Username, e.ChatMessage.UserId, e.ChatMessage.IsSubscriber);
                await StreamStatsService.UpdateStreamStat(1, StatTypes.MessagesReceived);

            }
            catch (Exception ex)
            {
                Log.Warning($"[Twitch Message] {ex}");

            }
        }

        private static async void UserBanned(object? sender, OnUserBannedArgs e)
        {
            await StreamStatsService.UpdateStreamStat(1, StatTypes.TotalBans);
            Log.Information($"[User Banned in Stream] User banned: {e.UserBan.Username}");
        }

        private static async void UserTimedOut(object? sender, OnUserTimedoutArgs e)
        {
            await StreamStatsService.UpdateStreamStat(1, StatTypes.TotalTimeouts);
            Log.Information($"[User Timed out in Stream] User banned: {e.UserTimeout.Username} Duration: {e.UserTimeout.TimeoutDuration} Reason: {e.UserTimeout.TimeoutReason}");
        }

        private static void MessageSent(object? sender, OnMessageSentArgs e)
        {
            Log.Information($"[Twitch Message Sent] {e.SentMessage.Message}");
        }

        private static async void GiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
        {
            await Subathon.AddSubathonSubTime(e.GiftedSubscription.MsgParamSubPlan, e.GiftedSubscription.DisplayName.ToLower());
            await Subscribers.HandleGiftSubscriptionEvent(e.GiftedSubscription.MsgParamSubPlan, e.GiftedSubscription.DisplayName, e.GiftedSubscription.MsgParamRecipientDisplayName, e.GiftedSubscription.UserId, e.GiftedSubscription.MsgParamRecipientId);
        }

        private static async void NewSubscriber(object? sender, OnNewSubscriberArgs e)
        {
            await Subathon.AddSubathonSubTime(e.Subscriber.SubscriptionPlan, e.Subscriber.DisplayName.ToLower());
            await Subscribers.HandleNewSubscriberEvent(e.Subscriber.SubscriptionPlan, e.Subscriber.DisplayName, e.Subscriber.UserId);
        }
    }
}
