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
    public class DailyPointsDataService(AppDbContext context, IConfigHelper configHelper, ITwitchHelperService twitchHelperService, IRecurringJobManager recurringJobManager) : IDailyPointsDataService
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

            var usersTest = await context.TwitchDailyPoints.ToListAsync();

            // reset streaks
            var usersToReset = await context.TwitchDailyPoints
                .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId && x.PointsClaimType == PointsClaimType.Daily && x.PointsClaimed == false)
                .ToListAsync();

            var top5LostStreaks = usersToReset
                .OrderByDescending(x => x.CurrentStreak)
                .Take(5)
                .ToList();

            foreach (var user in usersToReset)
            {
                user.CurrentStreak = 0;
            }

            await context.SaveChangesAsync();

            Log.Information($"Missed streaks reset - {usersToReset.Count} users reset");

            var rowsChanged = await context.TwitchDailyPoints
                .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId && x.PointsClaimType == PointsClaimType.Daily && x.PointsClaimed == true)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.PointsClaimed, false));

            Log.Information($"Reset {rowsChanged} users streaks");

            // allow the point collecting and let the users know
            await configHelper.UpdateDailyPointsStatus(broadcasterId, true);
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName!, $"Don't forget to claim your daily, weekly, monthly and yearly {await twitchHelperService.GetPointsName(broadcasterId, channelName!)} with !daily, !weekly, !monthly and !yearly PogChamp KEKW");
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName!, $"Top 5 lost streaks: {string.Join(", ", top5LostStreaks.Select(x => $"{x.User.TwitchUsername} - {x.CurrentStreak}"))}");

            // create a recurring job to remind the users to collect their points
            recurringJobManager.AddOrUpdate(
                $"DailyPointsCollectionReminder-{broadcasterId}",
                () => twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName!, $"Don't forget to claim your daily, weekly, monthly and yearly {twitchHelperService.GetPointsName(broadcasterId, channelName!)} with !daily, !weekly, !monthly and !yearly PogChamp KEKW", null),
                "*/30 * * * *"
            );
        }

        //TODO: test this
        public async Task<string> HandlePointsClaimed(ChannelChatMessageReceivedParams msgParams, PointsClaimType pointsClaimType)
        {
            var collectionAllowed = configHelper.GetDailyPointsStatus(msgParams.BroadcasterChannelId);
            var pointsName = await twitchHelperService.GetPointsName(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (!collectionAllowed.DailyPointsAllowed)
            {
                return $"Daily {pointsName} collection is not allowed at the moment! The stream has to be online for 30 minutes (ish) before claiming";
            }

            var user = await context.TwitchDailyPoints
                .FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.User.TwitchUserId == msgParams.ChatterChannelId && x.PointsClaimType == pointsClaimType);

            // If the user isn't registered in the database yet, return a friendly message
            if (user == null)
            {
                return $"Oops! Looks like you're not quite ready to claim your {pointsName} just yet. Give it a moment and try again!";
            }

            if (user.PointsClaimed)
            {
                return $"You silly sausage! You have claimed your {pointsClaimType.ToString().ToLower()} {pointsName}!";
            }

            //get the milestones for the points claim type
            var milestoneDict = pointsClaimType switch
            {
                PointsClaimType.Daily => new Dictionary<int, (int MinPoints, int MaxPoints)>
                {
                    { 100, (200000, 400000) },
                    { 50, (100000, 200000) },
                    { 25, (50000, 100000) },
                    { 10, (10000, 50000) }
                },
                PointsClaimType.Weekly => new Dictionary<int, (int MinPoints, int MaxPoints)>()
                {
                    { 10, (100000, 250000) }
                },
                PointsClaimType.Monthly => new Dictionary<int, (int MinPoints, int MaxPoints)>()
                {
                    { 6, (250000, 500000) }
                },
                PointsClaimType.Yearly => new Dictionary<int, (int MinPoints, int MaxPoints)>()
                {
                    { 1, (1, 1000000) }
                },
                _ => throw new InvalidDataException("Invalid PointsClaimType")
            };

            // check for milestones and give bonus points if applicable
            var bonusPoints = CheckForPointsMilestones(user.TotalTimesClaimed, user.CurrentStreak, pointsName!, pointsClaimType, milestoneDict);

            // work out the base points and add the bonus points if applicable
            var basePointsBonus = pointsClaimType switch
            {
                PointsClaimType.Daily => 250,
                PointsClaimType.Weekly => 1000,
                PointsClaimType.Monthly => 4000,
                PointsClaimType.Yearly => 52000,
                _ => throw new InvalidDataException("Invalid PointsClaimType")
            };

            var totalPoints = (user.CurrentStreak * basePointsBonus) + (bonusPoints?.BonusPoints ?? 0);

            await twitchHelperService.AddPointsToUser(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId, totalPoints, msgParams.BroadcasterChannelName, msgParams.ChatterChannelName);
            
            user.PointsClaimed = true;
            user.TotalPointsClaimed += totalPoints;
            user.CurrentStreak++;
            user.TotalTimesClaimed++;

            await context.SaveChangesAsync();

            return bonusPoints.HasValue ? bonusPoints.Value.ChatMessage : $"You have claimed your {pointsClaimType.ToString().ToLower()} {pointsName} for {totalPoints:N0} points! You are on a {user.CurrentStreak} day streak!";
        }

        //TODO: test this
        public async Task<string> HandleStreakCheckCommand(ChannelChatMessageReceivedParams msgParams, PointsClaimType pointsClaimType)
        {
            var pointsName = await twitchHelperService.GetPointsName(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            // Determine the target username
            var isCheckingAnotherUser = msgParams.MessageParts.Length > 1;
            var usernameToCheck = isCheckingAnotherUser ? msgParams.MessageParts[1].Trim().ToLower() : msgParams.ChatterChannelName;

            string? userId;

            if (isCheckingAnotherUser)
            {
                userId = await twitchHelperService.GetTwitchUserIdFromUsername(usernameToCheck);

                if (string.IsNullOrEmpty(userId))
                {
                    throw new TwitchUserNotFoundException("That was a bit of a silly thing to do! That user does not exist");
                }
            }
            else
            {
                userId = msgParams.ChatterChannelId;
            }

            var user = await context.TwitchDailyPoints
                .FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.User.TwitchUserId == userId && x.PointsClaimType == pointsClaimType);

            if (user == null)
            {
                return isCheckingAnotherUser
                    ? $"{usernameToCheck} hasn't started a streak yet!"
                    : "You haven't started a streak yet!";
            }

            return isCheckingAnotherUser
                ? $"{usernameToCheck} is on a {user.CurrentStreak} day streak!"
                : $"You are on a {user.CurrentStreak} day streak!";
        }

        private static (int BonusPoints, string ChatMessage)? CheckForPointsMilestones(int totalClaims, int currentStreak, string pointsName, PointsClaimType pointsClaimType, Dictionary<int, (int MinPoints, int MaxPoints)> streakMilestones)
        {
            foreach (var streakMilestone in streakMilestones)
            {
                if (totalClaims % streakMilestone.Key == 0)
                {
                    var randomPoints = new Random().Next(streakMilestone.Value.MinPoints, streakMilestone.Value.MaxPoints);
                    return (randomPoints, $"As this is your {totalClaims}th time claiming your {pointsClaimType.ToString().ToLower()} {pointsName}, you have been gifted an extra {randomPoints:N0} {pointsName} PogChamp !!! You are on a {currentStreak} day streak");
                }
            }

            return null;
        }
    }
}
