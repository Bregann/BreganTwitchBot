using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Web.Data.Leaderboards.Enums;
using Microsoft.EntityFrameworkCore;

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
                        users = context.Users.Include(x => x.UserGambleStats).OrderByDescending(x => x.UserGambleStats.PointsWon).Take(250).ToDictionary(x => x.Username, x => x.UserGambleStats.PointsWon);
                        break;
                    case LeaderboardType.PointsLost:
                        users = context.Users.Include(x => x.UserGambleStats).OrderByDescending(x => x.UserGambleStats.PointsLost).Take(250).ToDictionary(x => x.Username, x => x.UserGambleStats.PointsLost);
                        break;
                    case LeaderboardType.PointsGambled:
                        users = context.Users.Include(x => x.UserGambleStats).OrderByDescending(x => x.UserGambleStats.PointsGambled).Take(250).ToDictionary(x => x.Username, x => x.UserGambleStats.PointsGambled);
                        break;
                    case LeaderboardType.TotalSpins:
                        users = context.Users.Include(x => x.UserGambleStats).OrderByDescending(x => x.UserGambleStats.TotalSpins).Take(250).ToDictionary(x => x.Username, x => (long)x.UserGambleStats.TotalSpins);
                        break;
                    case LeaderboardType.CurrentStreak:
                        users = context.Users.Include(x => x.DailyPoints).OrderByDescending(x => x.DailyPoints.CurrentStreak).Take(250).ToDictionary(x => x.Username, x => (long)x.DailyPoints.CurrentStreak);
                        break;
                    case LeaderboardType.HighestStreak:
                        users = context.Users.Include(x => x.DailyPoints).OrderByDescending(x => x.DailyPoints.HighestStreak).Take(250).ToDictionary(x => x.Username, x => (long)x.DailyPoints.HighestStreak);
                        break;
                    case LeaderboardType.TotalTimesClaimed:
                        users = context.Users.Include(x => x.DailyPoints).OrderByDescending(x => x.DailyPoints.TotalTimesClaimed).Take(250).ToDictionary(x => x.Username, x => (long)x.DailyPoints.TotalTimesClaimed);
                        break;
                    case LeaderboardType.MarblesRacesWon:
                        users = context.Users.OrderByDescending(x => x.MarblesWins).Take(250).ToDictionary(x => x.Username, x => (long)x.MarblesWins);
                        break;
                    case LeaderboardType.DiscordStreak:
                        users = context.Users.Include(x => x.DailyPoints).OrderByDescending(x => x.DailyPoints.DiscordDailyTotalClaims).Take(250).ToDictionary(x => x.Username, x => (long)x.DailyPoints.DiscordDailyTotalClaims);
                        break;
                    case LeaderboardType.Discordlevel:
                        users = context.Users.Include(x => x.DiscordUserStats).OrderByDescending(x => x.DiscordUserStats.DiscordLevel).Take(250).ToDictionary(x => x.Username, x => (long)x.DiscordUserStats.DiscordLevel);
                        break;
                    case LeaderboardType.DiscordXP:
                        users = context.Users.Include(x => x.DiscordUserStats).OrderByDescending(x => x.DiscordUserStats.DiscordXp).Take(250).ToDictionary(x => x.Username, x => (long)x.DiscordUserStats.DiscordXp);
                        break;
                    case LeaderboardType.DiscordTotalClaims:
                        users = context.Users.Include(x => x.DailyPoints).OrderByDescending(x => x.DailyPoints.DiscordDailyTotalClaims).Take(250).ToDictionary(x => x.Username, x => (long)x.DailyPoints.DiscordDailyTotalClaims);
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
                        users = context.Users.Include(x => x.Watchtime).OrderByDescending(x => x.Watchtime.MinutesInStream).Take(250).ToDictionary(x => x.Username, x => x.Watchtime.MinutesInStream);
                        break;

                    case LeaderboardType.StreamHours:
                        users = context.Users.Include(x => x.Watchtime).OrderByDescending(x => x.Watchtime.MinutesWatchedThisStream).Take(250).ToDictionary(x => x.Username, x => x.Watchtime.MinutesWatchedThisStream);
                        break;

                    case LeaderboardType.WeeklyHours:
                        users = context.Users.Include(x => x.Watchtime).OrderByDescending(x => x.Watchtime.MinutesWatchedThisWeek).Take(250).ToDictionary(x => x.Username, x => x.Watchtime.MinutesWatchedThisWeek);
                        break;

                    case LeaderboardType.MonthlyHours:
                        users = context.Users.Include(x => x.Watchtime).OrderByDescending(x => x.Watchtime.MinutesWatchedThisMonth).Take(250).ToDictionary(x => x.Username, x => x.Watchtime.MinutesWatchedThisMonth);
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
                        Amount = user.Value.ToString() + " minutes" + $"({Math.Round((decimal)user.Value / 60, 2)} hours)"
                    });

                    pos++;
                }

                return lb;
            }
        }
    }
}