using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Hangfire;
using Serilog;

namespace BreganTwitchBot.Domain.Helpers
{
    public class HangfireJobServiceHelper(
        ITwitchApiConnection twitchApiConnection, 
        ITwitchHelperService twitchHelperService, 
        IHoursDataService hoursDataService, 
        IWordBlacklistMonitorService wordBlacklistMonitorService, 
        IDailyPointsDataService dailyPointsDataService, 
        IGeneralCommandsData generalCommandsData,
        IDiscordDailyPointsData discordDailyPointsData
        )
    {
        public void SetupHangfireJobs()
        {
            RecurringJob.AddOrUpdate("BigBenBong", () => BigBenBong(), "0 * * * *");
            RecurringJob.AddOrUpdate("TimeTrackerHoursUpdate", () => TimeTrackerHoursUpdate(), "* * * * *");
            RecurringJob.AddOrUpdate("RemoveWarnedUsers", () => RemoveWarnedUsers(), "* * * * *");
            RecurringJob.AddOrUpdate("ResetMinutes", () => ResetMinutes(), "0 3 * * *");
            RecurringJob.AddOrUpdate("DailyPointsReminder", () => AnnouncePointsReminderMessage(), "*/30 * * * *");
            RecurringJob.AddOrUpdate("ResetTwitchStreaks", () => ResetTwitchStreaks(), "0 2 * * *");
            RecurringJob.AddOrUpdate("RefreshApi", () => RefreshApi(), "45 * * * *");
            RecurringJob.AddOrUpdate("CheckBirthdays", () => CheckBirthdays(), "0 6 * * *");
            RecurringJob.AddOrUpdate("ResetDiscordStreaks", () => ResetDiscordStreaks(), "0 0 * * *");

            Log.Information("[Job Scheduler] Job Scheduler Setup");
        }

        public async Task BigBenBong()
        {
            var apiClients = twitchApiConnection.GetAllBotApiClients();

            if (apiClients == null)
            {
                Log.Error("[Hangfire Job Service] Error sending message to all channels, apiClients is null");
                return;
            }

            foreach (var bot in apiClients)
            {
                switch (TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "GMT Standard Time").Hour)
                {
                    case 1:
                    case 13:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕑 BONG");
                            break;
                        }
                    case 2:
                    case 14:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕑 BONG BONG");
                            break;
                        }
                    case 3:
                    case 15:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕒 BONG BONG BONG");
                            break;
                        }
                    case 4:
                    case 16:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕓 BONG BONG BONG BONG");
                            break;
                        }
                    case 5:
                    case 17:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕔 BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 6:
                    case 18:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕕 BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 7:
                    case 19:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕖 BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 8:
                    case 20:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕗 BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 9:
                    case 21:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕘 BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 10:
                    case 22:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕙 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 11:
                    case 23:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕚 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 12:
                    case 24:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕛 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                }

                Log.Information($"[Hangfire Job Service] Big Ben Bonged in channel name {bot.BroadcasterChannelName}");
            }
        }

        public async Task TimeTrackerHoursUpdate()
        {
            var channels = twitchApiConnection.GetAllBroadcasterChannelIds();

            foreach (var channelId in channels)
            {
                await hoursDataService.UpdateWatchtimeForChannel(channelId);
            }
        }

        public void RemoveWarnedUsers()
        {
            wordBlacklistMonitorService.RemoveWarnedUsers();
        }

        public async Task ResetMinutes()
        {
            await hoursDataService.ResetMinutes();
        }

        public async Task AnnouncePointsReminderMessage()
        {
            var channels = twitchApiConnection.GetAllBroadcasterChannelIds();

            foreach (var channelId in channels)
            {
                await dailyPointsDataService.AnnouncePointsReminder(channelId);
            }
        }

        public async Task ResetTwitchStreaks()
        {
            await dailyPointsDataService.ResetStreaks();
        }

        public async Task RefreshApi()
        {
            await twitchApiConnection.RefreshAllApiKeys();
        }

        public async Task CheckBirthdays()
        {
            await generalCommandsData.CheckForUserBirthdaysAndSendMessage();
        }

        public async Task ResetDiscordStreaks()
        {
            await discordDailyPointsData.ResetDiscordDailyStreaks();
        }
    }
}