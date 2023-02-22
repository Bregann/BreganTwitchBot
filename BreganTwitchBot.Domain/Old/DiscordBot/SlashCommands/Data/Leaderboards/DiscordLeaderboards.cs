using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.Leaderboards.Enums;

namespace BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.Leaderboards
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
                        return context.Users.OrderByDescending(x => x.CurrentStreak).Take(24).ToDictionary(x => x.Username, x => (long)x.CurrentStreak);

                    case DiscordLeaderboardType.DiscordLevel:
                        return context.Users.OrderByDescending(x => x.DiscordLevel).Take(24).ToDictionary(x => x.Username, x => (long)x.DiscordLevel);

                    case DiscordLeaderboardType.DiscordXp:
                        return context.Users.OrderByDescending(x => x.DiscordXp).Take(24).ToDictionary(x => x.Username, x => (long)x.DiscordXp);
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
                        return context.Users.OrderByDescending(x => x.MinutesWatchedThisStream).Take(24).ToDictionary(x => x.Username, x => (long)x.MinutesWatchedThisStream);

                    case DiscordLeaderboardType.WeeklyHours:
                        return context.Users.OrderByDescending(x => x.MinutesWatchedThisWeek).Take(24).ToDictionary(x => x.Username, x => (long)x.MinutesWatchedThisWeek);

                    case DiscordLeaderboardType.MonthlyHours:
                        return context.Users.OrderByDescending(x => x.BitsDonatedThisMonth).Take(24).ToDictionary(x => x.Username, x => (long)x.BitsDonatedThisMonth);

                    case DiscordLeaderboardType.AllTimeHours:
                        return context.Users.OrderByDescending(x => x.MinutesInStream).Take(24).ToDictionary(x => x.Username, x => (long)x.MinutesInStream);
                }

                //this will never be hit but causing the method to not work
                return null;
            }
        }
    }
}