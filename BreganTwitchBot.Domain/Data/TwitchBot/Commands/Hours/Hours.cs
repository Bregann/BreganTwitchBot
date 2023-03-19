using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Hours
{
    public class Hours
    {
        public static void HandleHoursCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (message.ToLower() == "!hours" || message.ToLower() == "!hrs" || message.ToLower() == "!houres" || message.ToLower() == "!watchtime")
            {
                var hours = GetUserTime(userId, HoursWatchTypes.AllTime);
                var hoursWatchedThisStream = GetUserTime(userId, HoursWatchTypes.Stream);

                TwitchHelper.SendMessage($"@{username} => You have {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) in the stream. You are {GetTimeTillNextRank(userId)} (Minutes watched this stream - {hoursWatchedThisStream.TotalMinutes} (about {Math.Round(hoursWatchedThisStream.TotalMinutes / 60, 2)} hours)) Rank: {GetHoursWatchedRank(userId, HoursWatchTypes.AllTime)}");
                return;
            }

            var otherUserId = TwitchHelper.GetUserIdFromUsername(chatArgumentsAsList[0].Replace("@", "").ToLower());

            if (otherUserId == null)
            {
                TwitchHelper.SendMessage($"@{username} => That user does not exist :(");
                return;
            }

            var otherUserHours = GetUserTime(otherUserId, HoursWatchTypes.AllTime);
            var otherUserHoursWatchedThisStream = GetUserTime(otherUserId, HoursWatchTypes.Stream);

            TwitchHelper.SendMessage($"@{username} => {chatArgumentsAsList[0].Replace("@", "").ToLower()} has {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) in the stream. (Minutes watched this stream - {otherUserHoursWatchedThisStream.TotalMinutes} (about {Math.Round(otherUserHoursWatchedThisStream.TotalMinutes / 60, 2)} hours))");
            Log.Information("[Twitch Commands] !hours command handled successfully");
        }

        public static void HandleStreamHoursCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (message.ToLower() == "!streamhours" || message.ToLower() == "!streamhrs")
            {
                var hours = GetUserTime(userId, HoursWatchTypes.Stream);

                TwitchHelper.SendMessage($"@{username} => You have watched {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) this stream! Rank: {GetHoursWatchedRank(userId, HoursWatchTypes.Stream)}");
                return;
            }

            var otherUserId = TwitchHelper.GetUserIdFromUsername(chatArgumentsAsList[0].Replace("@", "").ToLower());

            if (otherUserId == null)
            {
                TwitchHelper.SendMessage($"@{username} => That user does not exist :(");
                return;
            }

            var otherUserHours = GetUserTime(otherUserId, HoursWatchTypes.Stream);

            TwitchHelper.SendMessage($"@{username} => {chatArgumentsAsList[0].Replace("@", "").ToLower()} has watched {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) this stream!");
        }

        public static void HandleWeeklyHoursCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (message.ToLower() == "!weeklyhours" || message.ToLower() == "!weeklyhrs")
            {
                var hours = GetUserTime(userId, HoursWatchTypes.Week);

                TwitchHelper.SendMessage($"@{username} => You have watched {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) this week! Rank: {GetHoursWatchedRank(userId, HoursWatchTypes.Week)}");
                return;
            }

            var otherUserId = TwitchHelper.GetUserIdFromUsername(chatArgumentsAsList[0].Replace("@", "").ToLower());

            if (otherUserId == null)
            {
                TwitchHelper.SendMessage($"@{username} => That user does not exist :(");
                return;
            }

            var otherUserHours = GetUserTime(otherUserId, HoursWatchTypes.Week);

            TwitchHelper.SendMessage($"@{username} => {chatArgumentsAsList[0].Replace("@", "").ToLower()} has watched {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) this week!");
        }

        public static void HandleMonthlyHoursCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (message.ToLower() == "!monthlyhours" || message.ToLower() == "!monthlyhrs")
            {
                var hours = GetUserTime(userId, HoursWatchTypes.Month);

                TwitchHelper.SendMessage($"@{username} => You have watched {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) this month! Rank: {GetHoursWatchedRank(userId, HoursWatchTypes.Month)}");
                return;
            }

            var otherUserId = TwitchHelper.GetUserIdFromUsername(chatArgumentsAsList[0].Replace("@", "").ToLower());

            if (otherUserId == null)
            {
                TwitchHelper.SendMessage($"@{username} => That user does not exist :(");
                return;
            }

            var otherUserHours = GetUserTime(otherUserId, HoursWatchTypes.Month);

            TwitchHelper.SendMessage($"@{username} => {chatArgumentsAsList[0].Replace("@", "").ToLower()} has watched {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) this month!");
        }

        private static string GetHoursWatchedRank(string userId, HoursWatchTypes type)
        {
            using (var context = new DatabaseContext())
            {
                var position = new List<string>();
                switch (type)
                {
                    case HoursWatchTypes.Stream:
                        position = context.Watchtime.OrderByDescending(x => x.MinutesWatchedThisStream).Select(x => x.TwitchUserId).ToList();
                        break;

                    case HoursWatchTypes.Week:
                        position = context.Watchtime.OrderByDescending(x => x.MinutesWatchedThisWeek).Select(x => x.TwitchUserId).ToList();
                        break;

                    case HoursWatchTypes.Month:
                        position = context.Watchtime.OrderByDescending(x => x.MinutesWatchedThisMonth).Select(x => x.TwitchUserId).ToList();
                        break;

                    case HoursWatchTypes.AllTime:
                        position = context.Watchtime.OrderByDescending(x => x.MinutesInStream).Select(x => x.TwitchUserId).ToList();
                        break;
                }

                return $"{position.IndexOf(userId) + 1} / {position.Count}";
            }
        }

        private static TimeSpan GetUserTime(string userId, HoursWatchTypes type)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Watchtime.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return TimeSpan.FromMinutes(0);
                }

                return type switch
                {
                    HoursWatchTypes.Stream => TimeSpan.FromMinutes(user.MinutesWatchedThisStream),
                    HoursWatchTypes.Week => TimeSpan.FromMinutes(user.MinutesWatchedThisWeek),
                    HoursWatchTypes.Month => TimeSpan.FromMinutes(user.MinutesWatchedThisMonth),
                    HoursWatchTypes.AllTime => TimeSpan.FromMinutes(user.MinutesInStream),
                    _ => TimeSpan.FromMinutes(0)
                };
            }
        }

        private static string GetTimeTillNextRank(string userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Watchtime.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return "60 minutes away from melvin rank!";
                }

                if (user.MinutesInStream < 60)
                {
                    return $"{60 - user.MinutesInStream} minutes away from melvin rank!";
                }

                if (user.MinutesInStream >= 60 && user.MinutesInStream <= 1500)
                {
                    return $"{Math.Round((1500 - (double)user.MinutesInStream) / 60, 2)} hours away from WOT Crew!";
                }

                if (user.MinutesInStream >= 1500 && user.MinutesInStream <= 6000)
                {
                    return $"{Math.Round((6000 - (double)user.MinutesInStream) / 60, 2)} hours away from BLOCKS Crew!";
                }

                if (user.MinutesInStream >= 6000 && user.MinutesInStream <= 15000)
                {
                    return $"{Math.Round((15000 - (double)user.MinutesInStream) / 60, 2)} hours away from The Name of Legends!";
                }

                if (user.MinutesInStream >= 15000 && user.MinutesInStream <= 30000)
                {
                    return $"{Math.Round((30000 - (double)user.MinutesInStream) / 60, 2)} hours away from King of The Stream!";
                }

                return "in the stream too much lol (max rank)";
            }
        }
    }
}