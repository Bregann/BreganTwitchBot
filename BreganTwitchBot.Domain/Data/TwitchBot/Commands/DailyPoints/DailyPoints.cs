using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganUtils.ProjectMonitor.Projects;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.DailyPoints
{
    public class DailyPoints
    {
        public static async Task CheckLiveStreamStatus()
        {
            TimeSpan? uptime;
            try
            {
                var uptimeReq = await TwitchApiConnection.ApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { AppConfig.TwitchChannelID });

                if (uptimeReq.Streams.Length == 0)
                {
                    uptime = null;
                }
                else
                {
                    uptime = DateTime.UtcNow - uptimeReq.Streams[0].StartedAt;
                    ProjectMonitorBreganTwitchBot.SendStreamUptimeUpdate(DateTime.UtcNow - uptimeReq.Streams[0].StartedAt);
                }
            }
            catch (Exception e)
            {
                Log.Fatal($"[Daily Points Uptime Check] {e}");
                return;
            }

            //If the timespan is over 30 minutes then allow daily points collecting
            if (uptime != null)
            {
                if (uptime > TimeSpan.FromMinutes(30) && !AppConfig.DailyPointsCollectingAllowed)
                {
                    //Get the last stream and set it
                    DateTime lastStream;

                    using (var context = new DatabaseContext())
                    {
                        lastStream = context.Config.First().LastDailyPointsAllowed;
                    }

                    //dont reset if there's been a stream already today
                    if (DateTime.UtcNow.Date == lastStream.Date)
                    {
                        Log.Information("[Daily Points] There was already a stream today so don't need to reset the points");

                        //Allow the point collecting
                        AppConfig.UpdateDailyPointsCollecting(true);
                        TwitchHelper.SendMessage($"Don't forget to claim your daily, weekly, monthly and yearly {AppConfig.PointsName} with !daily, !weekly, !monthly and !yearly PogChamp KEKW (if you didn't collect them last stream :) )");
                        return;
                    }

                    //Reset the streaks
                    int amountOfUsersReset = 0;

                    using (var context = new DatabaseContext())
                    {
                        var usersToReset = context.DailyPoints.Where(x => x.PointsClaimedThisStream == false && x.CurrentStreak > 0);
                        var top5LostStreaks = context.DailyPoints
                            .Include(x => x.User)
                            .Where(x => x.PointsClaimedThisStream == false && x.CurrentStreak > 0)
                            .OrderByDescending(x => x.CurrentStreak)
                            .Take(5)
                            .ToDictionary(x => x.User.Username, x => x.CurrentStreak);
                        var usersToAllowCollecting = context.DailyPoints.Where(x => x.PointsClaimedThisStream == true);

                        foreach (var user in usersToReset)
                        {
                            user.CurrentStreak = 0;
                            amountOfUsersReset++;
                        }

                        foreach (var user in usersToAllowCollecting)
                        {
                            user.PointsClaimedThisStream = false;
                        }

                        context.DailyPoints.UpdateRange(usersToReset);
                        context.DailyPoints.UpdateRange(usersToAllowCollecting);
                        context.SaveChanges();


                        Log.Information($"[Daily Points] Missed streaks reset - {amountOfUsersReset} users reset");
                        StreamStatsService.UpdateStreamStat(amountOfUsersReset, StatTypes.AmountOfUsersReset);
                        Log.Information("[Daily Points] Points claimed set to false");

                        //Allow the point collecting
                        AppConfig.UpdateDailyPointsCollecting(true);

                        TwitchHelper.SendMessage($"Don't forget to claim your daily, weekly, monthly and yearly {AppConfig.PointsName} with !daily, !weekly, !monthly and !yearly PogChamp KEKW");

                        var message = "The top 5 lost streaks are: ";

                        foreach (var user in top5LostStreaks)
                        {
                            message += ($"{user.Key} - {user.Value} day streak, ");
                        }

                        TwitchHelper.SendMessage(message.Remove(message.Length - 2, 2));

                        Log.Information("[Daily Points] Daily points reminder started and point collecting is allowed");
                        return;
                    }
                }
            }

            //End when there is no uptime and the stream has been detected as offline
            if (uptime == null && AppConfig.DailyPointsCollectingAllowed && !AppConfig.StreamerLive)
            {
                AppConfig.UpdateDailyPointsCollecting(false);
                Log.Information("[Daily Points] Daily points can no longer be collected");
            }
        }

        public static async Task HandleClaimPointsCommand(string username, string userId, PointsClaimType pointsClaimType)
        {
            if (!AppConfig.DailyPointsCollectingAllowed)
            {
                TwitchHelper.SendMessage($"@{username} => You cannot claim your {AppConfig.PointsName} yet! The stream has to be online for 30 minutes (ish) before claiming");
                Log.Information("[Twitch Commands] !daily command handled (not yet)");
                return;
            }

            using (var context = new DatabaseContext())
            {
                //Get the user
                var user = context.DailyPoints.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                //if the user doesn't exist yet
                if (user == null)
                {
                    TwitchHelper.SendMessage($"@{username} => There has been an issue claiming your {AppConfig.PointsName}. Give it a minute and try again!");
                    Log.Information("[Twitch Commands] !daily command handled (not in db)");
                    return;
                }

                var pointsToAdd = 0;
                var randomPoints = 0;
                var bonusPointsMessage = "";
                var randomPointAmount = new Random();

                switch (pointsClaimType)
                {
                    case PointsClaimType.DailyPoints:
                        if (user.PointsClaimedThisStream)
                        {
                            TwitchHelper.SendMessage($"@{username} => You have already claimed your daily {AppConfig.PointsName} today!");
                            Log.Information("[Twitch Commands] !daily command handled (clamed already)");
                            return;
                        }

                        //handle the points claim
                        user.CurrentStreak++;
                        user.TotalTimesClaimed++;


                        //Update the users highest streak
                        if (user.CurrentStreak > user.HighestStreak)
                        {
                            user.HighestStreak = user.CurrentStreak;
                        }

                        var roundedStreakAmount = (int)(Math.Ceiling((decimal)user.CurrentStreak / 5) * 5);

                        //See if they are on a bonus
                        if (user.TotalTimesClaimed % 100 == 0)
                        {
                            randomPoints = randomPointAmount.Next(200000, 400000);
                            pointsToAdd = (roundedStreakAmount * 250) + randomPoints;
                            bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                        }
                        else if (user.TotalTimesClaimed % 50 == 0)
                        {
                            randomPoints = randomPointAmount.Next(100000, 200000);
                            pointsToAdd = (roundedStreakAmount * 250) + randomPoints;
                            bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                        }
                        else if (user.TotalTimesClaimed % 25 == 0)
                        {
                            randomPoints = randomPointAmount.Next(50000, 100000);
                            pointsToAdd = (roundedStreakAmount * 250) + randomPoints;
                            bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                        }
                        else if (user.TotalTimesClaimed % 10 == 0)
                        {
                            randomPoints = randomPointAmount.Next(10000, 50000);
                            pointsToAdd = (roundedStreakAmount * 250) + randomPoints;
                            bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                        }
                        else
                        {
                            pointsToAdd = roundedStreakAmount * 250;
                        }

                        StreamStatsService.UpdateStreamStat(1, StatTypes.TotalUsersClaimed);
                        StreamStatsService.UpdateStreamStat(pointsToAdd, StatTypes.TotalPointsClaimed);

                        await PointsHelper.AddUserPoints(userId, pointsToAdd);
                        user.PointsLastClaimed = DateTime.UtcNow;
                        user.PointsClaimedThisStream = true;
                        user.TotalPointsClaimed += pointsToAdd;

                        context.SaveChanges();

                        TwitchHelper.SendMessage($"@{username} => You have claimed your daily {pointsToAdd:N0} {AppConfig.PointsName}! {bonusPointsMessage} You are currently on a {user.CurrentStreak:N0} day streak");
                        Log.Information($"[Daily Points] {username} has claimed their points. Amount: {pointsToAdd}");
                        return;
                    case PointsClaimType.WeeklyPoints:
                        if (user.WeeklyClaimed)
                        {
                            TwitchHelper.SendMessage($"@{username} => You have already claimed your weekly {AppConfig.PointsName} this week!");
                            Log.Information("[Twitch Commands] !weekly command handled (clamed already)");
                            return;
                        }

                        //handle the points claim
                        user.CurrentWeeklyStreak++;
                        user.TotalWeeklyClaimed++;

                        //Update the users highest streak
                        if (user.CurrentWeeklyStreak > user.HighestWeeklyStreak)
                        {
                            user.HighestWeeklyStreak = user.CurrentWeeklyStreak;
                        }

                        //Users get a bonus for every 10 weeks claimed
                        if(user.TotalWeeklyClaimed % 10 == 0)
                        {
                            randomPoints = randomPointAmount.Next(100000, 250000);
                            pointsToAdd = (user.TotalWeeklyClaimed * 1000) + randomPoints;
                            bonusPointsMessage = $"As this is your {user.TotalWeeklyClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                        }
                        else
                        {
                            pointsToAdd = user.CurrentWeeklyStreak * 1000;
                        }

                        StreamStatsService.UpdateStreamStat(1, StatTypes.TotalUsersClaimed);
                        StreamStatsService.UpdateStreamStat(pointsToAdd, StatTypes.TotalPointsClaimed);

                        await PointsHelper.AddUserPoints(userId, pointsToAdd);
                        user.PointsLastClaimed = DateTime.UtcNow;
                        user.WeeklyClaimed = true;
                        user.TotalPointsClaimed += pointsToAdd;

                        context.SaveChanges();

                        TwitchHelper.SendMessage($"@{username} => You have claimed your weekly {pointsToAdd:N0} {AppConfig.PointsName}! {bonusPointsMessage} You are currently on a {user.CurrentWeeklyStreak:N0} week streak");
                        Log.Information($"[Daily Points] {username} has claimed their points. Amount: {pointsToAdd}");
                        return;
                    case PointsClaimType.MonthlyPoints:
                        if (user.MonthlyClaimed)
                        {
                            TwitchHelper.SendMessage($"@{username} => You have already claimed your monthly {AppConfig.PointsName} this month!");
                            Log.Information("[Twitch Commands] !monthly command handled (clamed already)");
                            return;
                        }

                        //handle the points claim
                        user.CurrentMonthlyStreak++;
                        user.TotalMonthlyClaimed++;

                        //Update the users highest streak
                        if (user.CurrentMonthlyStreak > user.HighestMonthlyStreak)
                        {
                            user.HighestMonthlyStreak = user.CurrentMonthlyStreak;
                        }

                        //Users get a bonus for every 6 months claimed
                        if (user.TotalMonthlyClaimed % 6 == 0)
                        {
                            randomPoints = randomPointAmount.Next(250000, 500000);
                            pointsToAdd = (user.TotalMonthlyClaimed * 2000) + randomPoints;
                            bonusPointsMessage = $"As this is your {user.TotalMonthlyClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                        }
                        else
                        {
                            pointsToAdd = user.TotalMonthlyClaimed * 2000;
                        }

                        StreamStatsService.UpdateStreamStat(1, StatTypes.TotalUsersClaimed);
                        StreamStatsService.UpdateStreamStat(pointsToAdd, StatTypes.TotalPointsClaimed);

                        await PointsHelper.AddUserPoints(userId, pointsToAdd);
                        user.PointsLastClaimed = DateTime.UtcNow;
                        user.MonthlyClaimed = true;
                        user.TotalPointsClaimed += pointsToAdd;

                        context.SaveChanges();

                        TwitchHelper.SendMessage($"@{username} => You have claimed your monthly {pointsToAdd:N0} {AppConfig.PointsName}! {bonusPointsMessage} You are currently on a {user.CurrentMonthlyStreak:N0} month streak");
                        Log.Information($"[Daily Points] {username} has claimed their points. Amount: {pointsToAdd}");
                        return;
                    case PointsClaimType.YearlyPoints:
                        if (user.YearlyClaimed)
                        {
                            TwitchHelper.SendMessage($"@{username} => You have already claimed your yearly {AppConfig.PointsName} this year!");
                            Log.Information("[Twitch Commands] !yearly command handled (clamed already)");
                            return;
                        }

                        //handle the points claim
                        user.CurrentYearlyStreak++;

                        //Update the users highest streak
                        if (user.CurrentYearlyStreak > user.HighestYearlyStreak)
                        {
                            user.HighestYearlyStreak = user.CurrentYearlyStreak;
                        }

                        //Users get a bonus every year
                        randomPoints = randomPointAmount.Next(1, 1000000);
                        pointsToAdd = (user.TotalYearlyClaimed * 10000) + randomPoints;

                        StreamStatsService.UpdateStreamStat(1, StatTypes.TotalUsersClaimed);
                        StreamStatsService.UpdateStreamStat(pointsToAdd, StatTypes.TotalPointsClaimed);

                        await PointsHelper.AddUserPoints(userId, pointsToAdd);
                        user.PointsLastClaimed = DateTime.UtcNow;
                        user.YearlyClaimed = true;
                        user.TotalPointsClaimed += pointsToAdd;

                        context.SaveChanges();

                        TwitchHelper.SendMessage($"@{username} => You have claimed your yearly {pointsToAdd:N0} {AppConfig.PointsName}! {bonusPointsMessage} You are currently on a {user.CurrentYearlyStreak:N0} year streak (you got {randomPoints:N0} bonus points)");
                        Log.Information($"[Daily Points] {username} has claimed their points. Amount: {pointsToAdd}");
                        return;
                }
            }
        }

        public static void HandleStreakCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (message.ToLower() == "!streak" || message.ToLower() == "!steak")
            {
                int currentStreak = 0;

                using (var context = new DatabaseContext())
                {
                    var user = context.DailyPoints.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                    if (user != null)
                    {
                        currentStreak = user.CurrentStreak;
                    }
                }

                TwitchHelper.SendMessage($"@{username} => You are on a {currentStreak:N0} day streak! Rank: {GetStreakRank(userId)}");
                return;
            }
            else
            {
                int currentStreak = 0;

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.Username == chatArgumentsAsList[0].ToLower().Replace("@", "")).FirstOrDefault();

                    if (user != null)
                    {
                        currentStreak = context.DailyPoints.Where(x => x.TwitchUserId == user.TwitchUserId).First().CurrentStreak;
                    }
                }

                TwitchHelper.SendMessage($"@{username} => {chatArgumentsAsList[0].Replace("@", "")} has a {currentStreak:N0} day streak!");
            }
        }

        private static string GetStreakRank(string userId)
        {
            using (var context = new DatabaseContext())
            {
                var position = context.DailyPoints.OrderByDescending(x => x.CurrentStreak).Select(x => x.TwitchUserId).ToList();
                return $"{position.IndexOf(userId) + 1} / {position.Count}";
            }
        }
    }
}