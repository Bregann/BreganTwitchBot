﻿using BreganTwitchBot.Domain.Data.DiscordBot;
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
            RecurringJob.AddOrUpdate("BigBenBong", () => BigBenBong(), "0 * * * *");
            RecurringJob.AddOrUpdate("CheckDailyPointsStreamStatus", () => CheckDailyPointsStreamStatus(), "*/30 * * * * *");
            RecurringJob.AddOrUpdate("RefreshApi", () => RefreshApi(), "45 * * * *");
            RecurringJob.AddOrUpdate("TimeTrackerHoursUpdate", () => TimeTrackerHoursUpdate(), "* * * * *"); //every minute
            RecurringJob.AddOrUpdate("GetStreamStatus", () => GetStreamStatus(), "*/20 * * * * *");
            RecurringJob.AddOrUpdate("StreamStatsViewerUpdate", () => StreamStatsViewerUpdate(), "* * * * *"); //every minute
            RecurringJob.AddOrUpdate("AnnounceDiscord", () => AnnounceDiscord(), "30 * * * *");
            RecurringJob.AddOrUpdate("ClearWarnedUsers", () => ClearWarnedUsers(), "*/5 * * * *");
            RecurringJob.AddOrUpdate("ResetMinutes", () => ResetMinutes(), "0 3 * * *");
            RecurringJob.AddOrUpdate("CheckBotConnectionState", () => CheckBotConnectionState(), "*/10 * * * * *");
            RecurringJob.AddOrUpdate("FollowerCheck", () => FollowerCheck(), "0 * * * *");
            RecurringJob.AddOrUpdate("GetDiscordMemberCount", () => GetDiscordMemberCount(), "*/10 * * * *");
            RecurringJob.AddOrUpdate("UpdateLeaderboardRoles", () => UpdateLeaderboardRoles(), "0 2 * * *");
            RecurringJob.AddOrUpdate("DiscordDailyReset", () => DiscordDailyReset(), "0 3 * * *");
            RecurringJob.AddOrUpdate("CheckBirthdays", () => CheckBirthdays(), "0 6 * * *");
            RecurringJob.AddOrUpdate("DailyPointsReminder", () => DailyPointsAnnouncementJob(), "20 * * * *");
            RecurringJob.AddOrUpdate("UpdateStatsInDatabase", () => UpdateStatsInDatabase(), "*/20 * * * * *");
            RecurringJob.AddOrUpdate("ResetTwitchStreaks", () => ResetTwitchStreaks(), "0 2 * * *");

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

        public static void DailyPointsAnnouncementJob()
        {
            if (AppConfig.DailyPointsCollectingAllowed)
            {
                TwitchHelper.SendMessage($"Don't forget to claim your daily {AppConfig.PointsName} with !daily, your weekly points with !weekly, your monthly points with !monthly and your yearly points with !yearly PogChamp");
            }
        }

        public static void StartDoublePingPreventionJob()
        {
            BackgroundJob.Schedule(() => DoublePingPrevention(), TimeSpan.FromMinutes(10));
        }

        public static void StartTwitchBossStreamAnnounceJob()
        {
            BackgroundJob.Schedule(() => TwitchBosses.StartBossFightCountdown(), TimeSpan.FromMinutes(60));
        }

        public static void BigBenBong()
        {
            switch (DateTimeHelper.ConvertDateTimeToLocalTime("GMT Standard Time", DateTime.UtcNow).Hour)
            {
                case 1:
                case 13:
                    {
                        TwitchHelper.SendMessage("🕐 BONG");
                        break;
                    }
                case 2:
                case 14:
                    {
                        TwitchHelper.SendMessage("🕑 BONG BONG");
                        break;
                    }
                case 3:
                case 15:
                    {
                        TwitchHelper.SendMessage("🕒 BONG BONG BONG");
                        break;
                    }
                case 4:
                case 16:
                    {
                        TwitchHelper.SendMessage("🕓 BONG BONG BONG BONG");
                        break;
                    }
                case 5:
                case 17:
                    {
                        TwitchHelper.SendMessage("🕔 BONG BONG BONG BONG BONG");
                        break;
                    }
                case 6:
                case 18:
                    {
                        TwitchHelper.SendMessage("🕕 BONG BONG BONG BONG BONG BONG");
                        break;
                    }

                case 7:
                case 19:
                    {
                        TwitchHelper.SendMessage("🕖 BONG BONG BONG BONG BONG BONG BONG");
                        break;
                    }

                case 8:
                case 20:
                    {
                        TwitchHelper.SendMessage("🕗 BONG BONG BONG BONG BONG BONG BONG BONG");
                        break;
                    }

                case 9:
                case 21:
                    {
                        TwitchHelper.SendMessage("🕘 BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                        break;
                    }
                case 10:
                case 22:
                    {
                        TwitchHelper.SendMessage("🕙 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                        break;
                    }

                case 11:
                case 23:
                    {
                        TwitchHelper.SendMessage("🕚 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                        break;
                    }
                case 12:
                case 24:
                    {
                        TwitchHelper.SendMessage("🕛 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                        break;
                    }
            }
        }

        public static async Task CheckDailyPointsStreamStatus()
        {
            await Data.TwitchBot.Commands.DailyPoints.DailyPoints.CheckLiveStreamStatus();
            return;
        }

        public static async Task RefreshApi()
        {
            try
            {
                var streamerRefresh = await TwitchApiConnection.ApiClient.Auth.RefreshAuthTokenAsync(AppConfig.BroadcasterRefresh, AppConfig.TwitchAPISecret, AppConfig.TwitchAPIClientID);
                TwitchApiConnection.ApiClient.Settings.AccessToken = streamerRefresh.AccessToken;
                AppConfig.UpdateStreamerApiCredentials(streamerRefresh.RefreshToken, streamerRefresh.AccessToken);

                Log.Information($"[Refresh Job] Streamer token {streamerRefresh.AccessToken} successfully refreshed! Expires in: {streamerRefresh.ExpiresIn} | Refresh: {streamerRefresh.RefreshToken}");

                var botRefresh = await TwitchApiConnection.ApiClient.Auth.RefreshAuthTokenAsync(AppConfig.TwitchBotApiRefresh, AppConfig.TwitchAPISecret, AppConfig.TwitchAPIClientID);
                TwitchApiConnection.ApiClient.Settings.AccessToken = botRefresh.AccessToken;
                AppConfig.UpdateBotApiCredentials(botRefresh.RefreshToken, botRefresh.AccessToken);

                Log.Information($"[Refresh Job] Bot token {botRefresh.AccessToken} successfully refreshed! Expires in: {botRefresh.ExpiresIn} | Refresh: {botRefresh.RefreshToken}");
            }
            catch (Exception e)
            {
                Log.Fatal($"[Refresh Job] Error refreshing {e}");
                return;
            }

            TwitchPubSubConnection.PubSubClient.SendTopics(AppConfig.BroadcasterOAuth);
        }

        public static async Task TimeTrackerHoursUpdate()
        {
            await Data.TwitchBot.Watchtime.UpdateUserWatchtime();
        }

        public static async Task GetStreamStatus()
        {
            await AppConfig.CheckAndUpdateIfStreamIsLive();
            return;
        }

        public static async Task StreamStatsViewerUpdate()
        {
            await StreamStatsService.GetUserListAndViewCountAndAddToTables();
            return;
        }

        public static void AnnounceDiscord()
        {
            if (AppConfig.StreamerLive)
            {
                TwitchHelper.SendMessage("💬 Make sure to join the Discord! https://discord.gg/jAjKtHZ");

                //I am lazy so sellout here🚨
                TwitchHelper.SendMessage("🚨🚨 Remember you can use code 'blocks' in the Hypixel store to support @blocksssssss 🚨🚨");
                return;
            }
        }

        public static void ClearWarnedUsers()
        {
            WordBlacklistData.ClearOutWarnedUsers();
        }

        public static void ResetMinutes()
        {
            if (DateTime.UtcNow.Day == 1)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var usersToReset = dbContext.Watchtime.Where(x => x.MinutesWatchedThisMonth != 0).ToList();

                    foreach (var user in usersToReset)
                    {
                        user.MinutesWatchedThisMonth = 0;
                    }

                    dbContext.SaveChanges();
                    Log.Information("[Minutes Job] Monthly minutes reset");
                }
            }

            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var usersToReset = dbContext.Watchtime.Where(x => x.MinutesWatchedThisWeek != 0).ToList();

                    foreach (var user in usersToReset)
                    {
                        user.MinutesWatchedThisWeek = 0;
                    }

                    dbContext.SaveChanges();
                    Log.Information("[Minutes Job] Weekly minutes reset");
                }
            }

            using (var dbContext = new DatabaseContext())
            {
                var usersToReset = dbContext.Watchtime.Where(x => x.MinutesWatchedThisStream != 0).ToList();

                foreach (var user in usersToReset)
                {
                    user.MinutesWatchedThisStream = 0;
                }

                dbContext.SaveChanges();
                Log.Information("[Minutes Job] Stream minutes reset");
            }
        }

        public static async Task CheckBotConnectionState()
        {
            if (TwitchBotConnection.Client.JoinedChannels.Count != 0)
            {
                _connectionsAttempt = 0;
                return;
            }

            while (_connectionsAttempt != 5)
            {
                try
                {
                    if (TwitchBotConnection.Client.JoinedChannels.Count == 0)
                    {
                        TwitchBotConnection.Client.Disconnect();
                        Log.Warning("[Disconnect Job] Bot has lost connection from the channel - disconnected in hope of reconnecting");
                        TwitchBotConnection.Client.Connect();
                        _connectionsAttempt++;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[Disconnect Job] Bot an error reconnecting to the Twitch chat - {e}");
                    _connectionsAttempt++;
                }
            }

            if (_connectionsAttempt == 5 && TwitchBotConnection.Client.JoinedChannels.Count == 0)
            {
                Log.Information("Bot will shutdown");
                await DiscordConnection.DiscordClient.LogoutAsync();
                await Task.Delay(10000);
                Environment.Exit(0);
                return;
            }
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

        public static async Task ResetTwitchStreaks()
        {
            using(var context = new DatabaseContext())
            {
                //Reset weekly streaks
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday && AppConfig.StreamHappenedThisWeek)
                {
                    await context.DailyPoints.Where(x => x.WeeklyClaimed == false && x.CurrentWeeklyStreak != 0).ExecuteUpdateAsync(x => x.SetProperty(s => s.CurrentWeeklyStreak, 0));
                    await context.DailyPoints.Where(x => x.WeeklyClaimed == true).ExecuteUpdateAsync(x => x.SetProperty(s => s.WeeklyClaimed, false));
                    Log.Information("[Daily Points] Weekly streaks reset");
                }

                //Reset monthly streaks
                if (DateTime.UtcNow.Day == 1)
                {
                    await context.DailyPoints.Where(x => x.MonthlyClaimed == false && x.CurrentMonthlyStreak != 0).ExecuteUpdateAsync(x => x.SetProperty(s => s.CurrentMonthlyStreak, 0));
                    await context.DailyPoints.Where(x => x.MonthlyClaimed == true).ExecuteUpdateAsync(x => x.SetProperty(s => s.MonthlyClaimed, false));
                    Log.Information("[Daily Points] Monthly streaks reset");
                }

                //Reset yearly streaks
                if (DateTime.UtcNow.Day == 1 && DateTime.UtcNow.Month == 1)
                {
                    await context.DailyPoints.Where(x => x.YearlyClaimed == false && x.CurrentYearlyStreak != 0).ExecuteUpdateAsync(x => x.SetProperty(s => s.CurrentYearlyStreak, 0));
                    await context.DailyPoints.Where(x => x.YearlyClaimed == true).ExecuteUpdateAsync(x => x.SetProperty(s => s.YearlyClaimed, false));
                    Log.Information("[Daily Points] Yearly streaks reset");
                }

                await context.SaveChangesAsync();
            }

            await AppConfig.SetStreamThisWeekToFalse();
        }
    }
}
