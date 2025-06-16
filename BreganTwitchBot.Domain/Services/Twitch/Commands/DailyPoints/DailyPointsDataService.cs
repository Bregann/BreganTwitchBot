using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.DailyPoints
{
    public class DailyPointsDataService(AppDbContext context, IConfigHelperService configHelper, ITwitchHelperService twitchHelperService, ITwitchApiConnection twitchApiConnection) : IDailyPointsDataService
    {
        /// <summary>
        /// Schedule daily points collection for the broadcaster. When the stream goes live a 30 minute timer is started to allow collecting points.
        /// </summary>
        /// <param name="broadcasterId"></param>
        /// <returns></returns>
        public async Task ScheduleDailyPointsCollection(string broadcasterId)
        {
            var dailyPointsStatus = configHelper.GetDailyPointsStatus(broadcasterId);
            var botUptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();

            // if it's within the last 20 minutes just allow collecting straight away as this could be due to a disconnection or something like that
            // also allow straight away for bot uptime less than 60 seconds
            if ((DateTime.UtcNow - dailyPointsStatus.LastStreamDate < TimeSpan.FromMinutes(20) && dailyPointsStatus.LastDailyPointedAllowedDate.Date == DateTime.UtcNow.Date) || botUptime.TotalSeconds < 60)
            {
                await AllowDailyPointsCollecting(broadcasterId);
                return;
            }

            BackgroundJob.Schedule(() => AllowDailyPointsCollecting(broadcasterId), TimeSpan.FromMinutes(30));
            Log.Information($"Scheduled daily points collection for {broadcasterId}");
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
            if (dailyPointsStatus.LastDailyPointedAllowedDate.Date == DateTime.UtcNow.Date)
            {
                await configHelper.UpdateDailyPointsStatus(broadcasterId, true);
                await twitchHelperService.SendAnnouncementMessageToChannel(broadcasterId, channelName!, $"Don't forget to claim your daily, weekly, monthly and yearly {await twitchHelperService.GetPointsName(broadcasterId, channelName!)} with !daily, !weekly, !monthly and !yearly PogChamp KEKW");
                Log.Information($"Daily points collection allowed for {broadcasterId} - stream happened today");
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
            await twitchHelperService.SendAnnouncementMessageToChannel(broadcasterId, channelName!, $"Don't forget to claim your daily, weekly, monthly and yearly {await twitchHelperService.GetPointsName(broadcasterId, channelName!)} with !daily, !weekly, !monthly and !yearly PogChamp KEKW");
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName!, $"Top 5 lost streaks: {string.Join(", ", top5LostStreaks.Select(x => $"{x.User.TwitchUsername} - {x.CurrentStreak}"))}");

            // send announcement message to the channel that points are allowed
            await AnnouncePointsReminder(broadcasterId);
        }

        public async Task AnnouncePointsReminder(string broadcasterId)
        {
            var dailyPointsStatus = configHelper.GetDailyPointsStatus(broadcasterId);
            var channelName = await twitchHelperService.GetTwitchUserIdFromUsername(broadcasterId);

            if (dailyPointsStatus.DailyPointsAllowed)
            {
                await twitchHelperService.SendAnnouncementMessageToChannel(broadcasterId, channelName!, $"Don't forget to claim your daily, weekly, monthly and yearly {await twitchHelperService.GetPointsName(broadcasterId, channelName!)} with !daily, !weekly, !monthly and !yearly PogChamp KEKW");
            }
        }

        public async Task ResetStreaks()
        {
            var channels = twitchApiConnection.GetAllBroadcasterChannelIds();

            // for weekly check if stream happened this week and only reset if it has
            foreach (var channelId in channels)
            {
                var config = configHelper.GetDailyPointsStatus(channelId);

                // check if the last stream was within the last 7 days, if it was then reset the streaks
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday && config.StreamHappenedThisWeek && config.LastStreamDate.Date.AddDays(7) >= DateTime.UtcNow.Date)
                {
                    var usersReset = await ResetStreaks(channelId, PointsClaimType.Weekly);
                    Log.Information($"Reset {usersReset} users streaks for channel {channelId} weekly");

                    // reset everybodies points claimed for the week where true
                    var rowsChanged = await ResetClaims(channelId, PointsClaimType.Weekly);
                    Log.Information($"Reset {rowsChanged} users to allow weekly points collecting for channel {channelId}");
                }

                //check if the stream has happened in the last month and only reset if it has
                if (DateTime.UtcNow.Day == 1 && config.LastStreamDate.Date.AddDays(30) >= DateTime.UtcNow.Date)
                {
                    var monthlyUsersReset = await ResetStreaks(channelId, PointsClaimType.Monthly);
                    Log.Information($"Reset {monthlyUsersReset} users streaks for monthly points");

                    // reset everybodies points claimed for the month where true
                    var rowsChanged = await ResetClaims(channelId, PointsClaimType.Monthly);
                    Log.Information($"Reset {rowsChanged} users to allow monthly points collecting");
                }

                // check if the stream has happened in the last year and only reset if it has
                if (DateTime.UtcNow.Month == 1 && DateTime.UtcNow.Day == 1 && config.LastStreamDate.Date.AddDays(365) >= DateTime.UtcNow.Date)
                {
                    var yearlyUsersReset = await ResetStreaks(channelId, PointsClaimType.Yearly);
                    Log.Information($"Reset {yearlyUsersReset} users streaks for yearly points");


                    // reset everybodies points claimed for the year where true
                    var rowsChanged = await ResetClaims(channelId, PointsClaimType.Yearly);
                    Log.Information($"Reset {rowsChanged} users to allow yearly points collecting");
                }
            }
        }

        private async Task<int> ResetStreaks(string broadcasterId, PointsClaimType claimType)
        {
            var rowsAffected = await context.TwitchDailyPoints
                        .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId && x.PointsClaimType == claimType && x.PointsClaimed == false)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.CurrentStreak, 0));

            return rowsAffected;
        }

        private async Task<int> ResetClaims(string broadcasterId, PointsClaimType claimType)
        {
            var rowsChanged = await context.TwitchDailyPoints
                        .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId && x.PointsClaimType == claimType && x.PointsClaimed == true)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.PointsClaimed, false));

            return rowsChanged;
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

            // increment the streak and total times claimed ready for milestone checking
            user.CurrentStreak++;
            user.TotalTimesClaimed++;
            user.PointsLastClaimed = DateTime.UtcNow;

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
            var bonusPoints = CheckForPointsMilestones(user.TotalTimesClaimed, pointsName!, pointsClaimType, milestoneDict);

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

            if (user.CurrentStreak > user.HighestStreak)
            {
                user.HighestStreak = user.CurrentStreak;
            }

            await context.SaveChangesAsync();

            return $"You have claimed your {pointsClaimType.ToString().ToLower()} {pointsName} for {totalPoints:N0} points! You are on a {user.CurrentStreak} {(pointsClaimType == PointsClaimType.Daily ? "day" : pointsClaimType.ToString().ToLower().TrimEnd(['l', 'y']))} streak!" + (bonusPoints.HasValue ? $" {bonusPoints.Value.ChatMessage}" : string.Empty);
        }

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
                ? $"{usernameToCheck} is on a {user.CurrentStreak} {(pointsClaimType == PointsClaimType.Daily ? "day" : pointsClaimType.ToString().ToLower().TrimEnd(['l', 'y']))} streak!"
                : $"You are on a {user.CurrentStreak} {(pointsClaimType == PointsClaimType.Daily ? "day" : pointsClaimType.ToString().ToLower().TrimEnd(['l', 'y']))} streak!";
        }

        private static (int BonusPoints, string ChatMessage)? CheckForPointsMilestones(int totalClaims, string pointsName, PointsClaimType pointsClaimType, Dictionary<int, (int MinPoints, int MaxPoints)> streakMilestones)
        {
            foreach (var streakMilestone in streakMilestones)
            {
                if (totalClaims % streakMilestone.Key == 0)
                {
                    var randomPoints = new Random().Next(streakMilestone.Value.MinPoints, streakMilestone.Value.MaxPoints);
                    var suffix = totalClaims % 10 == 1 && totalClaims % 100 != 11 ? "st" :
                                 totalClaims % 10 == 2 && totalClaims % 100 != 12 ? "nd" :
                                 totalClaims % 10 == 3 && totalClaims % 100 != 13 ? "rd" : "th";

                    return (randomPoints, $"As this is your {totalClaims}{suffix} time claiming your {pointsClaimType.ToString().ToLower()} {pointsName}, you have been gifted an extra {randomPoints:N0} {pointsName} PogChamp !!!");
                }
            }

            return null;
        }
    }
}
