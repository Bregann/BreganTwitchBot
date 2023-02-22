using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Services;
using BreganUtils.ProjectMonitor.Projects;
using Serilog;
using System.Text;
using TwitchLib.Client.Events;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DailyPoints
{
    public class DailyPoints
    {
        //todo: redo this method. Instead of checking every 1 min, start a job
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
                        HangfireJobs.CreateDailyPointsReminder();
                        return;
                    }

                    //Reset the streaks
                    int amountOfUsersReset = 0;

                    using (var context = new DatabaseContext())
                    {
                        var usersToReset = context.Users.Where(x => x.PointsClaimedThisStream == false && x.CurrentStreak > 0);
                        var usersToAllowCollecting = context.Users.Where(x => x.PointsClaimedThisStream == true);
                        var top5LostStreaks = context.Users.Where(x => x.PointsClaimedThisStream == false && x.CurrentStreak > 0).OrderByDescending(x => x.CurrentStreak).Take(5);

                        foreach (var user in usersToReset)
                        {
                            user.CurrentStreak = 0;
                            amountOfUsersReset++;
                        }

                        foreach (var user in usersToAllowCollecting)
                        {
                            user.PointsClaimedThisStream = false;
                        }

                        context.Users.UpdateRange(usersToReset);
                        context.Users.UpdateRange(usersToAllowCollecting);
                        context.SaveChanges();


                        Log.Information($"[Daily Points] Missed streaks reset - {amountOfUsersReset} users reset");
                        StreamStatsService.UpdateStreamStat(amountOfUsersReset, StatTypes.AmountOfUsersReset);
                        Log.Information("[Daily Points] Points claimed set to false");

                        //Allow the point collecting
                        AppConfig.UpdateDailyPointsCollecting(true);

                        HangfireJobs.CreateDailyPointsReminder();

                        /*var message = "The top 5 lost streaks are: ";

                        foreach (var user in top5LostStreaks)
                        {
                            message += ($"{user.Username} - {user.CurrentStreak} day streak, ");
                        }

                        TwitchHelper.SendMessage(message.Remove(message.Length - 1, 1));*/

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

        public static void HandleClaimPointsCommand(OnChatCommandReceivedArgs command)
        {
            if (!AppConfig.DailyPointsCollectingAllowed)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username.ToLower()} => You cannot claim your {AppConfig.PointsName} yet! The stream has to be online for 30 minutes (ish) before claiming");
                Log.Information("[Twitch Commands] !daily command handled (not yet)");
                return;
            }

            using (var context = new DatabaseContext())
            {
                //Get the user
                var user = context.Users.Where(x => x.Username == command.Command.ChatMessage.Username).FirstOrDefault();

                //if the user doesn't exist yet
                if (user == null)
                {
                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => There has been an issue claiming your {AppConfig.PointsName}. Give it a minute and try again!");
                    Log.Information("[Twitch Commands] !daily command handled (not in db)");
                    return;
                }

                if (user.PointsClaimedThisStream)
                {
                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You have already claimed your {AppConfig.PointsName} today!");
                    Log.Information("[Twitch Commands] !daily command handled (clamed already)");
                    return;
                }

                //handle the points claim
                user.CurrentStreak++;
                user.TotalTimesClaimed++;

                var pointsToAdd = 0;

                //Update the users highest streak
                if (user.CurrentStreak > user.HighestStreak)
                {
                    user.HighestStreak = user.CurrentStreak;
                }

                var randomPointAmount = new Random();
                var roundedStreakAmount = (int)(Math.Ceiling((decimal)user.CurrentStreak / 5) * 5);
                var bonusPointsMessage = "";

                //See if they are on a bonus
                if (user.TotalTimesClaimed % 100 == 0)
                {
                    var randomPoints = randomPointAmount.Next(200000, 400000);
                    pointsToAdd = roundedStreakAmount * 300 + randomPoints;
                    bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                }
                else if (user.TotalTimesClaimed % 50 == 0)
                {
                    var randomPoints = randomPointAmount.Next(100000, 200000);
                    pointsToAdd = roundedStreakAmount * 300 + randomPoints;
                    bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                }
                else if (user.TotalTimesClaimed % 25 == 0)
                {
                    var randomPoints = randomPointAmount.Next(50000, 100000);
                    pointsToAdd = roundedStreakAmount * 300 + randomPoints;
                    bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                }
                else if (user.TotalTimesClaimed % 10 == 0)
                {
                    var randomPoints = randomPointAmount.Next(10000, 50000);
                    pointsToAdd = roundedStreakAmount * 300 + randomPoints;
                    bonusPointsMessage = $"As this is your {user.TotalTimesClaimed}th time claiming you have been gifted an extra {randomPoints:N0} points PogChamp";
                }
                else
                {
                    pointsToAdd = roundedStreakAmount * 200;
                }

                StreamStatsService.UpdateStreamStat(1, StatTypes.TotalUsersClaimed);
                StreamStatsService.UpdateStreamStat(pointsToAdd, StatTypes.TotalPointsClaimed);

                user.Points += pointsToAdd;
                user.PointsLastClaimed = DateTime.UtcNow;
                user.PointsClaimedThisStream = true;
                user.TotalPointsClaimed += pointsToAdd;

                context.SaveChanges();

                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You have claimed your daily {roundedStreakAmount * 100:N0} {AppConfig.PointsName}! {bonusPointsMessage} You are currently on a {user.CurrentStreak:N0} day streak");
                Log.Information($"[Daily Points] {command.Command.ChatMessage.Username} has claimed their points. Amount: {pointsToAdd}");
            }
        }

        public static void HandleStreakCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message.ToLower() == "!streak" || command.Command.ChatMessage.Message.ToLower() == "!steak")
            {
                int currentStreak = 0;

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.Username == command.Command.ChatMessage.Username).FirstOrDefault();

                    if (user != null)
                    {
                        currentStreak = user.CurrentStreak;
                    }
                }

                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You are on a {currentStreak:N0} day streak! Rank: {GetStreakRank(command.Command.ChatMessage.Username.ToLower())}");
                return;
            }
            else
            {
                int currentStreak = 0;

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.Username == command.Command.ArgumentsAsList[0].ToLower().Replace("@", "")).FirstOrDefault();

                    if (user != null)
                    {
                        currentStreak = user.CurrentStreak;
                    }
                }

                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].Replace("@", "")} has a {currentStreak:N0} day streak!");
            }
        }

        private static string GetStreakRank(string username)
        {
            using (var context = new DatabaseContext())
            {
                var position = context.Users.OrderByDescending(x => x.CurrentStreak).Select(x => x.Username).ToList();
                return $"{position.IndexOf(username) + 1} / {position.Count}";
            }
        }
    }
}