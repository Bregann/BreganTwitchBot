using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using BreganTwitchBot.Services;
using Discord;
using Serilog;

namespace BreganTwitchBot.DiscordBot.Helpers
{
    public class DiscordHelper
    {
        public static async Task SendMessage(ulong channelID, string message)
        {
            try
            {
                var channel = await DiscordConnection.DiscordClient.GetChannelAsync(channelID) as IMessageChannel;
                await channel.TriggerTypingAsync();
                await channel.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Log.Fatal($"[Discord Message Send] Failed to send message - {e}");
                return;
            }
        }

        public static bool IsUserMod(ulong userId)
        {
            var user = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID).GetUser(userId);

            //null if it's a server announcement
            if (user == null)
            {
                return false;
            }

            var isMod = user.Roles.FirstOrDefault(x => x.Name == "Twitch Mot");

            return isMod != null;
        }

        public static async Task AnnounceStream()
        {
            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordStreamAnnouncementChannelID) as IMessageChannel;
            await channel.SendMessageAsync($"Hey @everyone! {AppConfig.BroadcasterName} is now live! Tune in at https://www.twitch.tv/{AppConfig.BroadcasterName} !");
        }

        public static async Task SendStreamStats()
        {
            StreamStats streamStats;

            using (var context = new DatabaseContext())
            {
                streamStats = context.StreamStats.OrderBy(x => x.StreamId).Last();
            }

            //Add the Discord stats into pages to send in the StreamStatsModule
            var generalStatsEmbed = new EmbedBuilder()
            {
                Title = "General stream stats",
                Description = $"Stream number: {streamStats.StreamId}\nUptime: {streamStats.Uptime}\nStream Started: {streamStats.StreamStarted}\nStream Ended: {streamStats.StreamEnded}\nAverage view count: {streamStats.AvgViewCount:N0}\nHighest view count: {streamStats.PeakViewerCount:N0}\nUnique people: {streamStats.UniquePeople:N0}\nStarting follower count: {streamStats.StartingFollowerCount:N0}\nEnding follower count: {streamStats.EndingFollowerCount:N0}\nFollower change: {streamStats.NewFollowers:N0}\nTotal bans: {streamStats.TotalBans:N0}\nTotal timeouts: {streamStats.TotalTimeouts:N0}\nDiscord ranks earned: {streamStats.DiscordRanksEarnt:N0}\nCommands Received: {streamStats.CommandsSent:N0}\nMessages Received: {streamStats.MessagesReceived:N0}\nChannel points rewards redeemed: {streamStats.AmountOfRewardsRedeemed:N0}\n Reward redeem cost: {streamStats.RewardRedeemCost:N0}"
            };

            var subsEmbed = new EmbedBuilder()
            {
                Title = "Subs and bits stats",
                Description = $"Starting sub count: {streamStats.StartingSubscriberCount:N0}\nEnding sub count: {streamStats.EndingSubscriberCount:N0}\nNew subscribers: {streamStats.NewSubscribers:N0}\nGifted Subs: {streamStats.NewGiftedSubs:N0}\nBits donated: {streamStats.BitsDonated:N0}"
            };

            var dailyPointsEmbed = new EmbedBuilder()
            {
                Title = "Daily points stats",
                Description = $"Total users claimed: {streamStats.TotalUsersClaimed:N0}\nTotal points claimed: {streamStats.TotalPointsClaimed:N0}\nTotal lost streaks: {streamStats.AmountOfUsersReset:N0}"
            };

            var discordStatsEmbed = new EmbedBuilder()
            {
                Title = "Discord stats",
                Description = $"Total users joined: {streamStats.AmountOfDiscordUsersJoined:N0}"
            };

            //Activate it
            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;
            await channel.TriggerTypingAsync();
            await channel.SendMessageAsync(embed: generalStatsEmbed.Build());
            await channel.SendMessageAsync(embed: subsEmbed.Build());
            await channel.SendMessageAsync(embed: dailyPointsEmbed.Build());
            await channel.SendMessageAsync(embed: discordStatsEmbed.Build());
        }

        public static void AddUserPoints(ulong discordId, long pointsToAdd)
        {
            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.DiscordUserId == discordId).FirstOrDefault().Points += pointsToAdd;
                context.SaveChanges();
            }
        }

        public static void RemoveUserPoints(ulong discordId, long pointsToRemove)
        {
            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.DiscordUserId == discordId).FirstOrDefault().Points -= pointsToRemove;
                context.SaveChanges();
            }
        }

        public static async Task InformDiscordIfUserBanned(string username)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

                if (user == null)
                {
                    return;
                }

                if (user.DiscordUserId != 0)
                {
                    await SendMessage(AppConfig.DiscordEventChannelID, $"<@&{AppConfig.DiscordBanRole}> DISCORD USER TO BAN - Banned in Twitch <@{user.DiscordUserId}>");
                }
            }
        }
    }
}