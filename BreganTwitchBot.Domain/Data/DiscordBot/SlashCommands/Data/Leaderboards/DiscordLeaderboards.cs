using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Leaderboards.Enums;
using BreganTwitchBot.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Leaderboards
{
    public class DiscordLeaderboards
    {
        public static Dictionary<string, long> GetLeaderboard(DiscordLeaderboardType type)
        {
            using (var context = new DatabaseContext())
            {
                switch (type)
                {
                    case DiscordLeaderboardType.Points:
                        return context.Users.OrderByDescending(x => x.Points).Take(24).ToDictionary(x => x.Username, x => x.Points);
                    case DiscordLeaderboardType.Marbles:
                        return context.Users.OrderByDescending(x => x.MarblesWins).Take(24).ToDictionary(x => x.Username, x => (long)x.MarblesWins);
                    case DiscordLeaderboardType.DailyStreak:
                        return context.Users.Include(x => x.DailyPoints).OrderByDescending(x => x.DailyPoints.CurrentStreak).Take(24).ToDictionary(x => x.Username, x => (long)x.DailyPoints.CurrentStreak);
                    case DiscordLeaderboardType.DiscordLevel:
                        return context.Users.Include(x => x.DiscordUserStats).OrderByDescending(x => x.DiscordUserStats.DiscordLevel).Take(24).ToDictionary(x => x.Username, x => (long)x.DiscordUserStats.DiscordLevel);
                    case DiscordLeaderboardType.DiscordXp:
                        return context.Users.Include(x => x.DiscordUserStats).OrderByDescending(x => x.DiscordUserStats.DiscordXp).Take(24).ToDictionary(x => x.Username, x => (long)x.DiscordUserStats.DiscordXp);
                }

                //this will never be hit but causing the method to not work
                return null;
            }
        }

        public static Dictionary<string, long> GetHoursLeaderboard(DiscordLeaderboardType type)
        {
            using (var context = new DatabaseContext())
            {
                var top5 = new Dictionary<string, long>();

                switch (type)
                {
                    case DiscordLeaderboardType.StreamHours:
                        return context.Users.Include(x => x.Watchtime).OrderByDescending(x => x.Watchtime.MinutesWatchedThisStream).Take(24).ToDictionary(x => x.Username, x => (long)x.Watchtime.MinutesWatchedThisStream);
                    case DiscordLeaderboardType.WeeklyHours:
                        return context.Users.Include(x => x.Watchtime).OrderByDescending(x => x.Watchtime.MinutesWatchedThisWeek).Take(24).ToDictionary(x => x.Username, x => (long)x.Watchtime.MinutesWatchedThisWeek);
                    case DiscordLeaderboardType.MonthlyHours:
                        return context.Users.OrderByDescending(x => x.BitsDonatedThisMonth).Take(24).ToDictionary(x => x.Username, x => (long)x.BitsDonatedThisMonth);
                    case DiscordLeaderboardType.AllTimeHours:
                        return context.Users.Include(x => x.Watchtime).OrderByDescending(x => x.Watchtime.MinutesInStream).Take(24).ToDictionary(x => x.Username, x => (long)x.Watchtime.MinutesInStream);
                }

                //this will never be hit but causing the method to not work
                return null;
            }
        }
    }
}