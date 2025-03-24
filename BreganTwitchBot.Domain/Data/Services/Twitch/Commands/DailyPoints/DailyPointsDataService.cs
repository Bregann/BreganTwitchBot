using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
                user.PointsClaimedThisStream = false;
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

        public async Task<string> HandlePointsClaimed(ChannelChatMessageReceivedParams msgParams, PointsClaimType pointsClaimType)
        {
            var collectionAllowed = configHelper.GetDailyPointsStatus(msgParams.BroadcasterChannelId);
            var pointsName = await twitchHelperService.GetPointsName(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (!collectionAllowed.DailyPointsAllowed)
            {
                return $"Daily {pointsName} collection is not allowed at the moment! The stream has to be online for 30 minutes (ish) before claiming";
            }

            var user = await context.TwitchDailyPoints
                .FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.User.TwitchUserId == msgParams.ChatterChannelId);

            // If the user isn't registered in the database yet, return a friendly message
            if (user == null)
            {
                return $"Oops! Looks like you're not quite ready to claim your {pointsName} just yet. Give it a moment and try again!";
            }

            var pointsClaimedAlready = pointsClaimType switch
            {
                PointsClaimType.Daily => user.PointsClaimedThisStream,
                PointsClaimType.Weekly => user.WeeklyPointsClaimed,
                PointsClaimType.Monthly => user.MonthlyPointsClaimed,
                PointsClaimType.Yearly => user.YearlyPointsClaimed,
                _ => throw new InvalidCommandException("dunno how you got to manage claiming your daily points this way, congrats you broke something")
            };

            if (pointsClaimedAlready)
            {
                return $"You silly sausage! You have claimed your {pointsClaimType.ToString().ToLower()} {pointsName}!";
            }

            switch (pointsClaimType)
            {
                case PointsClaimType.Daily:
                    var dict = new Dictionary<int, (int MinPoints, int MaxPoints)>
                    {

                    };

                    var pointsMilestoneMessage = await CheckForPointsMilestones(user.TotalClaims, msgParams.ChatterChannelId, pointsName, pointsClaimType);
                    break;
                case PointsClaimType.Weekly:
                    break;
                case PointsClaimType.Monthly:
                    break;
                case PointsClaimType.Yearly:
                    break;
                default:
                    throw new InvalidCommandException("dunno how you got to manage claiming your daily points this way, congrats you broke something");
            }
        }

        private async Task<string?> CheckForPointsMilestones(int totalClaims, string twitchUserId, string pointsName, PointsClaimType pointsClaimType, Dictionary<int, (int MinPoints, int MaxPoints)> streakMilestones)
        {
            foreach (var streakMilestone in streakMilestones)
            {
                if (totalClaims % streakMilestone.Key == 0)
                {
                    var randomPoints = new Random().Next(streakMilestone.Value.MinPoints, streakMilestone.Value.MaxPoints);
                    await twitchHelperService.AddPointsToUser(twitchUserId, twitchUserId, randomPoints, pointsName, twitchUserId);

                    return $"As this is your {totalClaims}th time claiming your {pointsClaimType.ToString().ToLower()} {pointsName}, you have been gifted an extra {randomPoints:N0} {pointsName} PogChamp !!!";
                }
            }

            return null;
        }
    }
}
