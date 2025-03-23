using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.DailyPoints
{
    public class DailyPointsDataService(AppDbContext context, IConfigHelper configHelper, ITwitchHelperService twitchHelperService) : IDailyPointsDataService
    {
        /// <summary>
        /// Schedule daily points collection for the broadcaster. When the stream goes live a 30 minute timer is started to allow collecting points.
        /// </summary>
        /// <param name="broadcasterId"></param>
        /// <returns></returns>
        public async Task ScheduleDailyPointsCollection(string broadcasterId)
        { 
            var dailyPointsStatus = configHelper.GetDailyPointsStatus(broadcasterId);

            // if it's within the last 20 minutes just allow collecting straight away as this could be due to a disconnection or something like that
            if (DateTime.UtcNow - dailyPointsStatus.LastStreamDate < TimeSpan.FromMinutes(20))
            {
                await AllowDailyPointsCollecting(broadcasterId);
                return;
            }

            BackgroundJob.Schedule(() => AllowDailyPointsCollecting(broadcasterId), TimeSpan.FromMinutes(30));
            Log.Information($"Scheduled daily points collection for {broadcasterId}");
        }

        /// <summary>
        /// Cancel daily points collection for the broadcaster
        /// </summary>
        /// <param name="broadcasterId"></param>
        public async Task CancelDailyPointsCollection(string broadcasterId)
        {
            RecurringJob.RemoveIfExists($"DailyPointsCollection-{broadcasterId}");
            RecurringJob.RemoveIfExists($"DailyPointsCollectionReminder-{broadcasterId}");

            await configHelper.UpdateDailyPointsStatus(broadcasterId, false);

            Log.Information($"Cancelled daily points collection for {broadcasterId}");
        }

        /// <summary>
        /// Allow daily points collecting for the broadcaster. Resets streaks that have been missed
        /// </summary>
        /// <param name="broadcasterId"></param>
        /// <returns></returns>
        public async Task AllowDailyPointsCollecting(string broadcasterId)
        {
            Log.Information($"Allowing daily points collection for {broadcasterId}");

            var dailyPointsStatus = configHelper.GetDailyPointsStatus(broadcasterId);
            var channelName = await twitchHelperService.GetTwitchUserIdFromUsername(broadcasterId);

            // don't reset streaks if there has been a stream today
            if (dailyPointsStatus.LastStreamDate.Date == DateTime.UtcNow.Date)
            {
                await configHelper.UpdateDailyPointsStatus(broadcasterId, true);
                await context.SaveChangesAsync();
                return;
            }

            // reset streaks
            var usersToReset = await context.TwitchDailyPoints
                .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId)
                .ToListAsync();

            var top5LostStreaks = usersToReset
                .OrderByDescending(x => x.CurrentDailyStreak)
                .Take(5)
                .ToList();

            foreach (var user in usersToReset)
            {
                user.CurrentDailyStreak = 0;
            }

            await context.SaveChangesAsync();

            Log.Information($"Missed streaks reset - {usersToReset.Count} users reset");

            // allow the point collecting and let the users know
            await configHelper.UpdateDailyPointsStatus(broadcasterId, true);
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName!, $"Don't forget to claim your daily, weekly, monthly and yearly {await twitchHelperService.GetPointsName(broadcasterId, channelName!)} with !daily, !weekly, !monthly and !yearly PogChamp KEKW");
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName!, $"Top 5 lost streaks: {string.Join(", ", top5LostStreaks.Select(x => $"{x.User.TwitchUsername} - {x.CurrentDailyStreak}"))}");

            // create a recurring job to remind the users to collect their points
            RecurringJob.AddOrUpdate(
                $"DailyPointsCollectionReminder-{broadcasterId}",
                () => twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName!, $"Don't forget to claim your daily, weekly, monthly and yearly {twitchHelperService.GetPointsName(broadcasterId, channelName!)} with !daily, !weekly, !monthly and !yearly PogChamp KEKW", null),
                "*/30 * * * *"
            );
        }
    }
}
