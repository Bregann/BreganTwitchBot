using BreganTwitchBot.Data;
using BreganTwitchBot.Data.Models;
using BreganTwitchBot.Web.Data.Leaderboards.Enums;

namespace BreganTwitchBot.Web.Data.Leaderboards
{
    public class LeaderboardsService
    {
        public List<Leaderboards> GetLeaderboard(LeaderboardType type)
        {
            using (var context = new DatabaseContext())
            {

                var lb = new List<Leaderboards>();
                var users = new Dictionary<string, long>();

                switch (type)
                {
                    case LeaderboardType.Points:
                        users = context.Users.OrderByDescending(x => x.Points).Take(250).ToDictionary(x => x.Username, x => x.Points);
                        break;
                    case LeaderboardType.PointsWon:
                        users = context.Users.OrderByDescending(x => x.PointsWon).Take(250).ToDictionary(x => x.Username, x => x.PointsWon);
                        break;
                    case LeaderboardType.PointsLost:
                        users = context.Users.OrderByDescending(x => x.PointsLost).Take(250).ToDictionary(x => x.Username, x => x.PointsLost);
                        break;
                    case LeaderboardType.PointsGambled:
                        users = context.Users.OrderByDescending(x => x.PointsGambled).Take(250).ToDictionary(x => x.Username, x => x.PointsGambled);
                        break;
                    case LeaderboardType.TotalSpins:
                        users = context.Users.OrderByDescending(x => x.TotalSpins).Take(250).ToDictionary(x => x.Username, x => (long)x.TotalSpins);
                        break;
                    case LeaderboardType.CurrentStreak:
                        users = context.Users.OrderByDescending(x => x.CurrentStreak).Take(250).ToDictionary(x => x.Username, x => (long)x.CurrentStreak);
                        break;
                    case LeaderboardType.HighestStreak:
                        users = context.Users.OrderByDescending(x => x.HighestStreak).Take(250).ToDictionary(x => x.Username, x => (long)x.HighestStreak);
                        break;
                    case LeaderboardType.TotalTimesClaimed:
                        users = context.Users.OrderByDescending(x => x.TotalTimesClaimed).Take(250).ToDictionary(x => x.Username, x => (long)x.TotalTimesClaimed);
                        break;
                    case LeaderboardType.MarblesRacesWon:
                        users = context.Users.OrderByDescending(x => x.MarblesWins).Take(250).ToDictionary(x => x.Username, x => (long)x.MarblesWins);
                        break;
                    case LeaderboardType.DiscordStreak:
                        users = context.Users.OrderByDescending(x => x.DiscordDailyTotalClaims).Take(250).ToDictionary(x => x.Username, x => (long)x.DiscordDailyTotalClaims);
                        break;
                    case LeaderboardType.Discordlevel:
                        users = context.Users.OrderByDescending(x => x.DiscordLevel).Take(250).ToDictionary(x => x.Username, x => (long)x.DiscordLevel);
                        break;
                    case LeaderboardType.DiscordXP:
                        users = context.Users.OrderByDescending(x => x.DiscordXp).Take(250).ToDictionary(x => x.Username, x => (long)x.DiscordXp);
                        break;
                    case LeaderboardType.DiscordTotalClaims:
                        users = context.Users.OrderByDescending(x => x.DiscordDailyTotalClaims).Take(250).ToDictionary(x => x.Username, x => (long)x.DiscordDailyTotalClaims);
                        break;
                    case LeaderboardType.MessagesSent:
                        users = context.Users.OrderByDescending(x => x.TotalMessages).Take(250).ToDictionary(x => x.Username, x => (long)x.TotalMessages);
                        break;
                    case LeaderboardType.BossesCompleted:
                        users = context.Users.OrderByDescending(x => x.BossesDone).Take(250).ToDictionary(x => x.Username, x => (long)x.BossesDone);
                        break;
                    default:
                        break;
                }

                int pos = 1;

                foreach (var user in users)
                {
                    lb.Add(new Leaderboards
                    {
                        Username = user.Key,
                        Position = pos,
                        Amount = user.Value
                    });

                    pos++;
                }

                return lb;
            }
        }

        public List<LeaderboardsHours> GetHourLeaderboard(LeaderboardType type)
        {
            using (var context = new DatabaseContext())
            {

                var lb = new List<LeaderboardsHours>();
                var users = new Dictionary<string, int>();

                switch (type)
                {
                    case LeaderboardType.AllTimeHours:
                        users = context.Users.OrderByDescending(x => x.MinutesInStream).Take(250).ToDictionary(x => x.Username, x => x.MinutesInStream);
                        break;
                    case LeaderboardType.StreamHours:
                        users = context.Users.OrderByDescending(x => x.MinutesWatchedThisStream).Take(250).ToDictionary(x => x.Username, x => x.MinutesWatchedThisStream);
                        break;
                    case LeaderboardType.WeeklyHours:
                        users = context.Users.OrderByDescending(x => x.MinutesWatchedThisWeek).Take(250).ToDictionary(x => x.Username, x => x.MinutesWatchedThisWeek);
                        break;
                    case LeaderboardType.MonthlyHours:
                        users = context.Users.OrderByDescending(x => x.MinutesWatchedThisMonth).Take(250).ToDictionary(x => x.Username, x => x.MinutesWatchedThisMonth);
                        break;
                    default:
                        break;
                }

                int pos = 1;

                foreach (var user in users)
                {
                    lb.Add(new LeaderboardsHours
                    {
                        Username = user.Key,
                        Position = pos,
                        Amount = user.Value.ToString() + " minutes" + $"({Math.Round((decimal)user.Value, 2)} hours)"
                    });

                    pos++;
                }

                return lb;
            }
        }
    }
}
