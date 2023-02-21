using BreganTwitchBot.Domain.Bot.Twitch.Services;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Events;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot.Stats;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            TwitchPubSubConnection.PubSubClient.OnChannelPointsRewardRedeemed += ChannelPointsRewardRedeemed;
            TwitchPubSubConnection.PubSubClient.OnBitsReceivedV2 += BitsReceived;
            TwitchPubSubConnection.PubSubClient.OnFollow += UserFollowed;
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
        }

        private static async void ChannelPointsRewardRedeemed(object? sender, OnChannelPointsRewardRedeemedArgs e)
        {
            await ChannelPoints.HandleChannelPointsEvent(e.RewardRedeemed.Redemption.Reward.Title, e.RewardRedeemed.Redemption.Reward.Cost, e.RewardRedeemed.Redemption.User.DisplayName, e.RewardRedeemed.Redemption.Status);
        }

        private static void UserFollowed(object? sender, OnFollowArgs e)
        {
            Log.Information($"[New Twitch Follow] {e.DisplayName} just followed! Twitch ID: {e.UserId}");
        }
    }
}
