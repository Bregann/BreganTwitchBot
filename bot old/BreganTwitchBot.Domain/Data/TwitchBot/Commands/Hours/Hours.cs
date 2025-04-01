using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Hours
{
    public class Hours
    {
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