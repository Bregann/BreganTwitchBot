using BreganTwitchBot.Domain.Data.DiscordBot;
using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.TwitchBosses;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot.WordBlacklist;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganUtils;
using Discord;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TwitchLib.Client.Extensions;

namespace BreganTwitchBot.Domain
{
    public class HangfireJobs
    {
        private static int _connectionsAttempt;
        private static long _currentFollowerCount;
        private static bool _raidFollowersJobStarted;

        public static void SetupHangfireJobs()
        {
            RecurringJob.AddOrUpdate("StreamStatsViewerUpdate", () => StreamStatsViewerUpdate(), "* * * * *"); //every minute
            RecurringJob.AddOrUpdate("FollowerCheck", () => FollowerCheck(), "0 * * * *");
            RecurringJob.AddOrUpdate("GetDiscordMemberCount", () => GetDiscordMemberCount(), "*/10 * * * *");
            RecurringJob.AddOrUpdate("UpdateLeaderboardRoles", () => UpdateLeaderboardRoles(), "0 2 * * *");
            RecurringJob.AddOrUpdate("DiscordDailyReset", () => DiscordDailyReset(), "0 3 * * *");
            RecurringJob.AddOrUpdate("CheckBirthdays", () => CheckBirthdays(), "0 6 * * *");
            RecurringJob.AddOrUpdate("UpdateStatsInDatabase", () => UpdateStatsInDatabase(), "*/20 * * * * *");

            //todo: add a job for every 5 mins, get active chatters and update users in stream
            Log.Information("[Job Scheduler] Job Scheduler Setup");
        }

        public static void DeleteDailyPointsReminderJob()
        {
            RecurringJob.RemoveIfExists("DailyPointsReminder");
        }

        public static void StartRaidFollowersOffJob()
        {
            if (_raidFollowersJobStarted)
            {
                Log.Information("[Raid Job] Raid job already running");
                return;
            }

            TwitchBotConnection.Client.FollowersOnlyOff(AppConfig.BroadcasterName);
            Log.Information("[Raid Job] Followers only turned off");

            BackgroundJob.Schedule(() => TurnFollowersOn(), TimeSpan.FromMinutes(2));

            _raidFollowersJobStarted = true;
        }

        public static void StartDoublePingPreventionJob()
        {
            BackgroundJob.Schedule(() => DoublePingPrevention(), TimeSpan.FromMinutes(10));
        }

        public static void StartTwitchBossStreamAnnounceJob()
        {
            BackgroundJob.Schedule(() => TwitchBosses.StartBossFightCountdown(), TimeSpan.FromMinutes(60));
        }

        public static async Task StreamStatsViewerUpdate()
        {
            await StreamStatsService.GetUserListAndViewCountAndAddToTables();
            return;
        }

        public static async Task FollowerCheck()
        {
            try
            {
                var newFollowerCount = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: AppConfig.TwitchChannelID);

                if (newFollowerCount.TotalFollows - _currentFollowerCount == 0)
                {
                    Log.Information($"[Hourly Follow Count Checker] No follower change. Current follower count -> {_currentFollowerCount}");
                    _currentFollowerCount = newFollowerCount.TotalFollows;
                    return;
                }

                var messageEmbed = new EmbedBuilder()
                {
                    Title = "Follow count",
                    Timestamp = DateTime.Now,
                    Color = new Discord.Color(0, 217, 22)
                };

                messageEmbed.AddField("Before follower count", _currentFollowerCount.ToString());
                messageEmbed.AddField("Current follower count", newFollowerCount.TotalFollows.ToString());
                messageEmbed.AddField("Change", (newFollowerCount.TotalFollows - _currentFollowerCount).ToString());

                var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;

                if (channel != null)
                {
                    await channel.SendMessageAsync(embed: messageEmbed.Build());
                }

                _currentFollowerCount = newFollowerCount.TotalFollows;
            }
            catch (Exception e)
            {
                Log.Fatal($"[Hourly Follow Count Checker] An error has occured {e}");
                return;
            }

            return;
        }

        public static async Task GetDiscordMemberCount()
        {
            var guildMembers = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            await DiscordConnection.DiscordClient.SetGameAsync($"{guildMembers.MemberCount} members", null, ActivityType.Watching);
            Log.Information($"[Discord Member Count] Discord member status updated to {guildMembers} members");
            return;
        }

        public static async Task UpdateLeaderboardRoles()
        {
            await Leaderboards.UpdateTheRanks();
        }

        public static async Task DiscordDailyReset()
        {
            using (var dbContext = new DatabaseContext())
            {
                var usersToUpdate = dbContext.DailyPoints.Where(x => x.DiscordDailyClaimed == true).ToList();

                foreach (var user in usersToUpdate)
                {
                    user.DiscordDailyClaimed = false;
                }

                dbContext.SaveChanges();
            }

            //Also prune the discord lol
            await DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID).PruneUsersAsync(7);
        }

        public static async Task CheckBirthdays()
        {
            using (var dbContext = new DatabaseContext())
            {
                var birthdays = dbContext.Birthdays.Where(x => x.Day == DateTime.UtcNow.Day && x.Month == DateTime.UtcNow.Month).ToList();

                if (birthdays.Count == 0)
                {
                    return;
                }

                foreach (var user in birthdays)
                {
                    if (user.DiscordId == AppConfig.DiscordGuildOwnerID)
                    {
                        await DiscordHelper.SendMessage(AppConfig.DiscordGeneralChannel, $"@everyone It's <@{user.DiscordId}> dumble birthday today! Happy Birthday <@{user.DiscordId}>! Make sure to ask him if he needs a nero :)");
                    }
                    else
                    {
                        await DiscordHelper.SendMessage(AppConfig.DiscordGeneralChannel, $"It's <@{user.DiscordId}> birthday today! Happy Birthday <@{user.DiscordId}>!");
                    }
                }
            }

            return;
        }

        public static void StartTwitchBossFight()
        {
            BackgroundJob.Schedule(() => TwitchBosses.StartBossFight(), TimeSpan.FromMinutes(2));
        }

        public static void DoublePingPrevention()
        {
            if (AppConfig.StreamerLive)
            {
                Log.Information("[Stream Check] Stream probably went offline then online again");
                return;
            }

            AppConfig.SetStreamAnnouncedToFalse();
            DeleteDailyPointsReminderJob();
            Log.Information("[Stream Check] Stream is offline after 10 mins");
        }

        public static void TurnFollowersOn()
        {
            TwitchBotConnection.Client.FollowersOnlyOn(AppConfig.BroadcasterName, TimeSpan.FromSeconds(0));
            _raidFollowersJobStarted = false;
            Log.Information("[Raid Job] Followers only turned on");
        }

        public static async Task UpdateStatsInDatabase()
        {
            await StreamStatsService.UpdateStatsInDatabase();
        }
    }
}
