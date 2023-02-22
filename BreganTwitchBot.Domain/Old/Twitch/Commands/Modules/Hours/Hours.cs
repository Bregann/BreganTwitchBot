using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Hours.Enums;
using Serilog;
using TwitchLib.Client.Events;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Hours
{
    public class Hours
    {
        public static void HandleHoursCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message.ToLower() == "!hours" || command.Command.ChatMessage.Message.ToLower() == "!hrs" || command.Command.ChatMessage.Message.ToLower() == "!houres")
            {
                var hours = GetUserTime(command.Command.ChatMessage.Username, HoursWatchTypes.AllTime);
                var hoursWatchedThisStream = GetUserTime(command.Command.ChatMessage.Username, HoursWatchTypes.Stream);

                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You have {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) in the stream. You are {GetTimeTillNextRank(command.Command.ChatMessage.Username.ToLower())} (Minutes watched this stream - {hoursWatchedThisStream.TotalMinutes} (about {Math.Round(hoursWatchedThisStream.TotalMinutes / 60, 2)} hours)) Rank: {GetHoursWatchedRank(command.Command.ChatMessage.Username.ToLower(), HoursWatchTypes.AllTime)}");
                return;
            }

            var otherUserHours = GetUserTime(command.Command.ArgumentsAsList[0].Replace("@", "").ToLower(), HoursWatchTypes.AllTime);
            var otherUserHoursWatchedThisStream = GetUserTime(command.Command.ArgumentsAsList[0].Replace("@", "").ToLower(), HoursWatchTypes.Stream);

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].Replace("@", "").ToLower()} has {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) in the stream. (Minutes watched this stream - {otherUserHoursWatchedThisStream.TotalMinutes} (about {Math.Round(otherUserHoursWatchedThisStream.TotalMinutes / 60, 2)} hours))");
            Log.Information("[Twitch Commands] !hours command handled successfully");
        }

        public static void HandleStreamHoursCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message.ToLower() == "!streamhours" || command.Command.ChatMessage.Message.ToLower() == "!streamhrs")
            {
                var hours = GetUserTime(command.Command.ChatMessage.Username, HoursWatchTypes.Stream);

                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You have watched {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) this stream! Rank: {GetHoursWatchedRank(command.Command.ChatMessage.Username.ToLower(), HoursWatchTypes.Stream)}");
                return;
            }

            var otherUserHours = GetUserTime(command.Command.ArgumentsAsList[0].Replace("@", "").ToLower(), HoursWatchTypes.Stream);

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].Replace("@", "").ToLower()} has watched {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) this stream!");
        }

        public static void HandleWeeklyHoursCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message.ToLower() == "!weeklyhours" || command.Command.ChatMessage.Message.ToLower() == "!weeklyhrs")
            {
                var hours = GetUserTime(command.Command.ChatMessage.Username, HoursWatchTypes.Week);

                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You have watched {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) this week! Rank: {GetHoursWatchedRank(command.Command.ChatMessage.Username.ToLower(), HoursWatchTypes.Week)}");
                return;
            }

            var otherUserHours = GetUserTime(command.Command.ArgumentsAsList[0].Replace("@", "").ToLower(), HoursWatchTypes.Week);

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].Replace("@", "").ToLower()} has watched {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) this week!");
        }

        public static void HandleMonthlyHoursCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message.ToLower() == "!monthlyhours" || command.Command.ChatMessage.Message.ToLower() == "!monthlyhrs")
            {
                var hours = GetUserTime(command.Command.ChatMessage.Username, HoursWatchTypes.Month);

                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You have watched {hours.TotalMinutes} minutes (about {Math.Round(hours.TotalMinutes / 60, 2)} hours) this month! Rank: {GetHoursWatchedRank(command.Command.ChatMessage.Username.ToLower(), HoursWatchTypes.Month)}");
                return;
            }

            var otherUserHours = GetUserTime(command.Command.ArgumentsAsList[0].Replace("@", "").ToLower(), HoursWatchTypes.Month);

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].Replace("@", "").ToLower()} has watched {otherUserHours.TotalMinutes} minutes (about {Math.Round(otherUserHours.TotalMinutes / 60, 2)} hours) this month!");
        }

        private static string GetHoursWatchedRank(string username, HoursWatchTypes type)
        {
            using (var context = new DatabaseContext())
            {
                var position = new List<string>();
                switch (type)
                {
                    case HoursWatchTypes.Stream:
                        position = context.Users.OrderByDescending(x => x.MinutesWatchedThisStream).Select(x => x.Username).ToList();
                        break;

                    case HoursWatchTypes.Week:
                        position = context.Users.OrderByDescending(x => x.MinutesWatchedThisWeek).Select(x => x.Username).ToList();
                        break;

                    case HoursWatchTypes.Month:
                        position = context.Users.OrderByDescending(x => x.MinutesWatchedThisMonth).Select(x => x.Username).ToList();
                        break;

                    case HoursWatchTypes.AllTime:
                        position = context.Users.OrderByDescending(x => x.MinutesInStream).Select(x => x.Username).ToList();
                        break;
                }

                return $"{position.IndexOf(username)} / {position.Count}";
            }
        }

        private static TimeSpan GetUserTime(string username, HoursWatchTypes type)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

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

        //todo: make the rank names a config variable or enum
        private static string GetTimeTillNextRank(string username)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

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