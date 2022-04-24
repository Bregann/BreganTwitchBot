using BreganTwitchBot.Core.DiscordBot.Services;
using BreganTwitchBot.Core.Twitch.Commands.Modules.DailyPoints;
using BreganTwitchBot.Core.Twitch.Commands.Modules.TwitchBosses;
using BreganTwitchBot.Core.Twitch.Commands.Modules.WordBlacklist;
using BreganTwitchBot.Core.Twitch.Services;
using BreganTwitchBot.Data;
using BreganTwitchBot.Twitch.Helpers;
using Discord;
using Quartz;
using Quartz.Impl;
using Serilog;
using TwitchLib.Client.Extensions;

namespace BreganTwitchBot.Services
{
    public class JobScheduler
    {
        private static StdSchedulerFactory _factory;
        private static IScheduler _scheduler;

        public static async Task SetupJobScheduler()
        {
            //construct the scheduler factory
            _factory = new StdSchedulerFactory();
            _scheduler = await _factory.GetScheduler();
            await _scheduler.Start();

            //Big Ben Bong
            var bongHourTrigger = TriggerBuilder.Create().WithIdentity("bongHourTrigger").WithCronSchedule("0 0 0/1 1/1 * ? *").Build();
            var bonger = JobBuilder.Create<BigBenBongerJob>().WithIdentity("bong").Build();

            //Check daily points
            var checkDailyPointsLiveStreamStatus = JobBuilder.Create<CheckDailyPointsStreamStatus>().WithIdentity("checkDailyPointsLiveStreamStatus").Build();
            var checkDailyPointsLiveStreamStatusTrigger = TriggerBuilder.Create().WithIdentity("checkDailyPointsLiveStreamStatusTrigger").WithSimpleSchedule(x => x.WithIntervalInSeconds(20).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(5)).Build();

            //Api refreshing
            var apiRefreshTrigger = TriggerBuilder.Create().WithIdentity("apiRefreshTrigger").WithCronSchedule("0 45 * ? * * *").WithPriority(1).Build();
            var apiRefresh = JobBuilder.Create<RefreshApiJob>().WithIdentity("apiRefresh").Build();

            //Update users for hours
            var timeTrackerHoursUpdate = JobBuilder.Create<TimeTrackerHoursUpdateJob>().WithIdentity("timeTrackerHoursUpdate").Build();
            var timeTrackerHoursUpdateTrigger = TriggerBuilder.Create().WithIdentity("timeTrackerHoursUpdateTrigger").WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(20)).Build();

            //Get the stream status
            var getStreamStatus = JobBuilder.Create<GetStreamStatusJob>().WithIdentity("getStreamStatus").Build();
            var getStreamStatusTrigger = TriggerBuilder.Create().WithIdentity("getStreamStatusTrigger").WithSimpleSchedule(x => x.WithIntervalInSeconds(20).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(10)).Build();

            //Update stream stats
            var streamStatsViewerUpdate = JobBuilder.Create<StreamStatsViewerUpdateJob>().WithIdentity("streamStatsViewerUpdate").Build();
            var streamStatsViewerUpdateTrigger = TriggerBuilder.Create().WithIdentity("streamStatsViewerUpdateTrigger").WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(20)).Build();

            //Announce discord
            var announceDiscord = JobBuilder.Create<AnnounceDiscord>().WithIdentity("announceDiscord").Build();
            var announceDiscordTrigger = TriggerBuilder.Create().WithIdentity("announceDiscordTrigger").WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(30)).Build();

            //Clear warned users
            var warnedUsers = JobBuilder.Create<ClearWarnedUsersJob>().WithIdentity("warnedUsers").Build();
            var warnedUsersTrigger = TriggerBuilder.Create().WithIdentity("warnedUsersTrigger").WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(30)).Build();

            //Reset weekly/monthly minutes
            var resetMinutesTrigger = TriggerBuilder.Create().WithIdentity("resetMinutesTrigger").WithCronSchedule("0 0 3 1/1 * ? *").Build();
            var resetMinutes = JobBuilder.Create<ResetMinutesJob>().WithIdentity("resetMinutes").Build();

            //Check if bot is connected to twitch
            var checkBotConnectionState = JobBuilder.Create<CheckBotConnectionStateJob>().WithIdentity("checkBotConnectionState").Build();
            var checkBotConnectionStateTrigger = TriggerBuilder.Create().WithIdentity("checkBotConnectionStateTrigger").WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(30)).Build();

            #region Discord Jobs
            //follower count check
            var followCheck = TriggerBuilder.Create().WithIdentity("followCheck").WithCronSchedule("0 0 0/1 1/1 * ? *").StartAt(DateTimeOffset.Now.AddSeconds(60)).Build();
            var followCheckTrigger = JobBuilder.Create<FollowerCheckJob>().WithIdentity("followCheckTrigger").Build();

            //Get Discord member count
            var getDiscordMemberCount = JobBuilder.Create<GetDiscordMemberCount>().WithIdentity("getDiscordMemberCount").Build();
            var getDiscordMemberCountTrigger = TriggerBuilder.Create().WithIdentity("getDiscordMemberCountTrigger").WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(30)).Build();

            //Leaderboards
            var leaderboardsTrigger = TriggerBuilder.Create().WithIdentity("leaderboardsTrigger").WithCronSchedule("0 0 2 1/1 * ? *").Build();
            var leaderboards = JobBuilder.Create<UpdateLeaderboardRolesJob>().WithIdentity("leaderboards").Build();

            //Discord dailies
            var discordDailiesTrigger = TriggerBuilder.Create().WithIdentity("discordDailiesTrigger").WithCronSchedule("0 0 3 1/1 * ? *").Build();
            var discordDailies = JobBuilder.Create<DiscordDailyResetJob>().WithIdentity("discordDailies").Build();
            #endregion

            await _scheduler.ScheduleJob(bonger, bongHourTrigger);
            await _scheduler.ScheduleJob(checkDailyPointsLiveStreamStatus, checkDailyPointsLiveStreamStatusTrigger);
            await _scheduler.ScheduleJob(apiRefresh, apiRefreshTrigger);
            await _scheduler.ScheduleJob(timeTrackerHoursUpdate, timeTrackerHoursUpdateTrigger);
            await _scheduler.ScheduleJob(getStreamStatus, getStreamStatusTrigger);
            await _scheduler.ScheduleJob(streamStatsViewerUpdate, streamStatsViewerUpdateTrigger);
            await _scheduler.ScheduleJob(announceDiscord, announceDiscordTrigger);
            await _scheduler.ScheduleJob(warnedUsers, warnedUsersTrigger);
            await _scheduler.ScheduleJob(resetMinutes, resetMinutesTrigger);
            await _scheduler.ScheduleJob(checkBotConnectionState, checkBotConnectionStateTrigger);
            await _scheduler.ScheduleJob(followCheckTrigger, followCheck);
            await _scheduler.ScheduleJob(getDiscordMemberCount, getDiscordMemberCountTrigger);
            await _scheduler.ScheduleJob(leaderboards, leaderboardsTrigger);
            await _scheduler.ScheduleJob(discordDailies, discordDailiesTrigger);

            Log.Information("[Job Scheduler] Job Scheduler Setup");
        }

        public static async Task CreateDailyPointsReminder()
        {
            var dailyPointsReminder = JobBuilder.Create<SendDailyPointsReminderJob>().WithIdentity("dailyPointsReminder").Build();
            var dailyPointsReminderTrigger = TriggerBuilder.Create().WithIdentity("dailyPointsReminderTrigger").WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever()).StartAt(DateTimeOffset.Now.AddSeconds(5)).Build();
            await _scheduler.ScheduleJob(dailyPointsReminder, dailyPointsReminderTrigger);
        }

        public static async Task StartTwitchBoss() //for starting the fight
        {
            var twitchBoss = JobBuilder.Create<StartTwitchBossJob>().WithIdentity("twitchBoss").Build();
            var twitchBossTrigger = TriggerBuilder.Create().WithIdentity("twitchBossTrigger").StartAt(DateTimeOffset.Now.AddMinutes(2)).Build();
            await _scheduler.ScheduleJob(twitchBoss, twitchBossTrigger);
        }

        public static async Task CreateDoublePingPreventionJob()
        {
            var doublePingPreventionJob = JobBuilder.Create<DoublePingPreventionJob>().WithIdentity("doublePingPrevention").Build();
            var doublePingPreventionJobTrigger = TriggerBuilder.Create().WithIdentity("doublePingPreventionTrigger").StartAt(DateTimeOffset.Now.AddMinutes(10)).Build();
            await _scheduler.ScheduleJob(doublePingPreventionJob, doublePingPreventionJobTrigger);
            Log.Information("[Double Ping Prevention] Job started");
        }

        public static async Task StartRaidFollowersOffJob()
        {
            var jobKey = new JobKey("raidFollowerOffJob");
            if (await _scheduler.CheckExists(jobKey))
            {
                Log.Information("[Raid Job] Raid job already running");
                return;
            }

            TwitchBotConnection.Client.FollowersOnlyOff(Config.BroadcasterName);
            Log.Information("[Raid Job] Followers only turned off");

            var raidFollowerOffJob = JobBuilder.Create<TurnFollowersOnJob>().WithIdentity("raidFollowerOffJob").Build();
            var raidFollowerOffTrigger = TriggerBuilder.Create().WithIdentity("raidFollowerOffTrigger").StartAt(DateTimeOffset.Now.AddMinutes(2)).Build();
            await _scheduler.ScheduleJob(raidFollowerOffJob, raidFollowerOffTrigger);
        }

        public static async Task StartTwitchBossStreamAnnounceJob() //for start of stream
        {
            var twitchBoss = JobBuilder.Create<StartTwitchCountdownJob>().WithIdentity("startTwitchBossDelay").Build();
            var twitchBossTrigger = TriggerBuilder.Create().WithIdentity("startTwitchBossDelayTrigger").StartAt(DateTimeOffset.Now.AddMinutes(60)).Build();
            await _scheduler.ScheduleJob(twitchBoss, twitchBossTrigger);
        }

        public static async Task DisposeDailyPointsReminder()
        {
            await _scheduler.DeleteJob(new JobKey("dailyPointsReminder"));
            Log.Information("[Daily Points] Daily points reminder disposed");
        }
    }

    internal class BigBenBongerJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            switch (DateTime.Now.Hour)
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

            return Task.CompletedTask;
        }
    }

    internal class AnnounceDiscord : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (Config.StreamerLive)
            {
                TwitchHelper.SendMessage("💬 Make sure to join the Discord! https://discord.gg/jAjKtHZ");

                //I am lazy so sellout here🚨
                TwitchHelper.SendMessage("🚨🚨 Remember you can use code 'blocks' in the Hypixel store to support @blocksssssss 🚨🚨");
                return;
            }
        }
    }

    internal class SendDailyPointsReminderJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            TwitchHelper.SendMessage($"Don't forget to claim your daily {Config.PointsName} with !daily PogChamp");
            return Task.CompletedTask;
        }
    }

    internal class CheckDailyPointsStreamStatus : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await DailyPoints.CheckLiveStreamStatus();
            return;
        }
    }

    internal class RefreshApiJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var refresh = await TwitchApiConnection.ApiClient.Auth.RefreshAuthTokenAsync(Config.BroadcasterRefresh, Config.TwitchAPISecret, Config.TwitchAPIClientID);
                TwitchApiConnection.ApiClient.Settings.AccessToken = refresh.AccessToken;
                Config.UpdateStreamConfig(refresh.RefreshToken, refresh.AccessToken);

                Log.Information($"[Refresh Job] Token {refresh.AccessToken} successfully refreshed! Expires in: {refresh.ExpiresIn} | Refresh: {refresh.RefreshToken}");
            }
            catch (Exception e)
            {
                Log.Fatal($"[Refresh Job] Error refreshing {e}");
                return;
            }

            //reset pusub
            try
            {
                TwitchPubSubConnection.PubSubClient.Disconnect();
                TwitchPubSubConnection.PubSubClient.ListenToBitsEventsV2(Config.TwitchChannelID);
                TwitchPubSubConnection.PubSubClient.ListenToFollows(Config.TwitchChannelID);
                TwitchPubSubConnection.PubSubClient.ListenToSubscriptions(Config.TwitchChannelID);
                TwitchPubSubConnection.PubSubClient.ListenToChannelPoints(Config.TwitchChannelID);
                TwitchPubSubConnection.PubSubClient.Connect();
            }
            catch (AggregateException e)
            {
                Log.Fatal($"[Refresh Job] Error disconnecting from pubsub {e}");
                var pubSub = new TwitchPubSubConnection();
                pubSub.Connect();
            }
        }
    }

    internal class TimeTrackerHoursUpdateJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Watchtime.UpdateUserWatchtime();
        }
    }

    internal class StartTwitchBossJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await TwitchBosses.StartBossFight();
        }
    }

    internal class DoublePingPreventionJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (Config.StreamerLive)
            {
                Log.Information("[Stream Check] Stream probably went offline then online again");
                return;
            }

            Config.SetStreamAnnouncedToFalse();
            await JobScheduler.DisposeDailyPointsReminder();
            Log.Information("[Stream Check] Stream is offline after 10 mins");
            return;
        }
    }

    internal class GetStreamStatusJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Config.CheckAndUpdateIfStreamIsLive();
            return;
        }
    }

    internal class StreamStatsViewerUpdateJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await StreamStatsService.GetUserListAndViewCountAndAddToTables();
            return;
        }
    }

    internal class TurnFollowersOnJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            TwitchBotConnection.Client.FollowersOnlyOn(Config.BroadcasterName, TimeSpan.FromSeconds(0));
            Log.Information("[Raid Job] Followers only turned on");
        }
    }

    internal class StartTwitchCountdownJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await TwitchBosses.StartBossFightCountdown();
        }
    }

    internal class ClearWarnedUsersJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            WordBlacklist.ClearOutWarnedUsers();
        }
    }

    internal class ResetMinutesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (DateTime.Now.Day == 1)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var usersToReset = dbContext.Users.Where(x => x.MinutesWatchedThisMonth != 0).ToList();

                    foreach (var user in usersToReset)
                    {
                        user.MinutesWatchedThisMonth = 0;
                    }

                    dbContext.Users.UpdateRange(usersToReset);
                    dbContext.SaveChanges();
                    Log.Information("[Minutes Job] Monthly minutes reset");
                }
            }

            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var usersToReset = dbContext.Users.Where(x => x.MinutesWatchedThisWeek != 0).ToList();

                    foreach (var user in usersToReset)
                    {
                        user.MinutesWatchedThisWeek = 0;
                    }

                    dbContext.Users.UpdateRange(usersToReset);
                    dbContext.SaveChanges();
                    Log.Information("[Minutes Job] Weekly minutes reset");
                }
            }
        }
    }

    internal class CheckBotConnectionStateJob : IJob
    {
        //Not a fan of this code but it works lol
        private static int _connectionsAttempt;

        public async Task Execute(IJobExecutionContext context)
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
    }

    internal class FollowerCheckJob : IJob
    {
        private static long _currentFollowerCount;

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var newFollowerCount = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: Config.TwitchChannelID);

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
                    Color = new Color(0, 217, 22)
                };

                messageEmbed.AddField("Before follower count", _currentFollowerCount.ToString());
                messageEmbed.AddField("Current follower count", newFollowerCount.TotalFollows.ToString());
                messageEmbed.AddField("Change", (newFollowerCount.TotalFollows - _currentFollowerCount).ToString());

                var channel = await DiscordConnection.DiscordClient.GetChannelAsync(Config.DiscordEventChannelID) as IMessageChannel;
                await channel.SendMessageAsync(embed: messageEmbed.Build());

                _currentFollowerCount = newFollowerCount.TotalFollows;
            }
            catch (Exception e)
            {
                Log.Fatal($"[Hourly Follow Count Checker] An error has occured {e}");
                return;
            }

            return;
        }
    }

    internal class GetDiscordMemberCount : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var guildMembers = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
            await DiscordConnection.DiscordClient.SetGameAsync($"{guildMembers.MemberCount} members", null, ActivityType.Watching);
            Log.Information($"[Discord Member Count] Discord member status updated to {guildMembers} members");
            return;
        }
    }

    internal class UpdateLeaderboardRolesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Leaderboards.UpdateTheRanks();
        }
    }


    internal class DiscordDailyResetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var usersToUpdate = dbContext.Users.Where(x => x.DiscordDailyClaimed == true).ToList();

                foreach (var user in usersToUpdate)
                {
                    user.DiscordDailyClaimed = false;
                }

                dbContext.Users.UpdateRange(usersToUpdate);
                dbContext.SaveChanges();
            }

            return;
        }
    }
}
