using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Leaderboards.Enums;
using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using System.Text;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Leaderboards
{
    public class Leaderboards
    {
        public static void HandleLeaderboardCommand(string username, LeaderboardType type)
        {
            switch (type)
            {
                case LeaderboardType.Points:
                    TwitchHelper.SendMessage($"@{username} => Top 5 points - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me/points");
                    break;

                case LeaderboardType.AllTimeHours:
                    TwitchHelper.SendMessage($"@{username} => Top 5 hours watched all time - {GetHoursLeaderboard(type)} Find the top 250 at https://bot.bregan.me/pointswon");
                    break;

                case LeaderboardType.StreamHours:
                    TwitchHelper.SendMessage($"@{username} => Top 5 hours watched this stream - {GetHoursLeaderboard(type)} Find the top 250 at https://bot.bregan.me/pointswon");
                    break;

                case LeaderboardType.WeeklyHours:
                    TwitchHelper.SendMessage($"@{username} => Top 5 hours watched this week - {GetHoursLeaderboard(type)} Find the top 250 at https://bot.bregan.me/pointswon");
                    break;

                case LeaderboardType.MonthlyHours:
                    TwitchHelper.SendMessage($"@{username} => Top 5 hours watched this month - {GetHoursLeaderboard(type)} Find the top 250 at https://bot.bregan.me/pointswon");
                    break;

                case LeaderboardType.PointsWon:
                    TwitchHelper.SendMessage($"@{username} => Top 5 points won gambling- {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me/pointswon");
                    break;

                case LeaderboardType.PointsLost:
                    TwitchHelper.SendMessage($"@{username} => Top 5 points lost gambling - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me/pointslost");
                    break;

                case LeaderboardType.PointsGambled:
                    TwitchHelper.SendMessage($"@{username} => Top 5 points gambled - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me/pointsgambled");
                    break;

                case LeaderboardType.TotalSpins:
                    TwitchHelper.SendMessage($"@{username} => Top 5 total spins - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me/totalspins");
                    break;

                case LeaderboardType.CurrentStreak:
                    TwitchHelper.SendMessage($"@{username} => Top 5 current daily streak - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me/currentstreak");
                    break;

                case LeaderboardType.HighestStreak:
                    TwitchHelper.SendMessage($"@{username} => Top 5 highest daily streak - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me");
                    break;

                case LeaderboardType.TotalTimesClaimed:
                    TwitchHelper.SendMessage($"@{username} => Top 5 total dailies claimed - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me/totalclaims");
                    break;

                case LeaderboardType.MarblesRacesWon:
                    TwitchHelper.SendMessage($"@{username} => Top 5 marbols GP wins - {GetLeaderboard(type)} Find the top 250 at https://bot.bregan.me");
                    break;

                default:
                    break;
            }
        }

        private static string GetLeaderboard(LeaderboardType type)
        {
            using (var context = new DatabaseContext())
            {
                var top5 = new Dictionary<string, long>();

                switch (type)
                {
                    case LeaderboardType.Points:
                        top5 = context.Users.OrderByDescending(x => x.Points).Take(5).ToDictionary(x => x.Username, x => x.Points);
                        break;

                    case LeaderboardType.PointsWon:
                        top5 = context.Users.OrderByDescending(x => x.PointsWon).Take(5).ToDictionary(x => x.Username, x => x.PointsWon);
                        break;

                    case LeaderboardType.PointsLost:
                        top5 = context.Users.OrderByDescending(x => x.PointsLost).Take(5).ToDictionary(x => x.Username, x => x.PointsLost);
                        break;

                    case LeaderboardType.PointsGambled:
                        top5 = context.Users.OrderByDescending(x => x.PointsGambled).Take(5).ToDictionary(x => x.Username, x => x.PointsGambled);
                        break;

                    case LeaderboardType.TotalSpins:
                        top5 = context.Users.OrderByDescending(x => x.TotalSpins).Take(5).ToDictionary(x => x.Username, x => (long)x.TotalSpins);
                        break;

                    case LeaderboardType.CurrentStreak:
                        top5 = context.Users.OrderByDescending(x => x.CurrentStreak).Take(5).ToDictionary(x => x.Username, x => (long)x.CurrentStreak);
                        break;

                    case LeaderboardType.HighestStreak:
                        top5 = context.Users.OrderByDescending(x => x.HighestStreak).Take(5).ToDictionary(x => x.Username, x => (long)x.HighestStreak);
                        break;

                    case LeaderboardType.TotalTimesClaimed:
                        top5 = context.Users.OrderByDescending(x => x.TotalTimesClaimed).Take(5).ToDictionary(x => x.Username, x => (long)x.TotalTimesClaimed);
                        break;

                    case LeaderboardType.MarblesRacesWon:
                        top5 = context.Users.OrderByDescending(x => x.MarblesWins).Take(5).ToDictionary(x => x.Username, x => (long)x.MarblesWins);
                        break;

                    default:
                        break;
                }

                var usersandItemSb = new StringBuilder();
                var position = 1;

                foreach (var user in top5)
                {
                    var username = $"#{position} - {user.Key} - {user.Value:N0} | ";
                    usersandItemSb.Append(username);
                    position++;
                }

                return usersandItemSb.ToString();
            }
        }

        private static string GetHoursLeaderboard(LeaderboardType type)
        {
            using (var context = new DatabaseContext())
            {
                var top5 = new Dictionary<string, long>();

                switch (type)
                {
                    case LeaderboardType.StreamHours:
                        top5 = context.Users.OrderByDescending(x => x.MinutesWatchedThisStream).Take(4).ToDictionary(x => x.Username, x => (long)x.MinutesWatchedThisStream);
                        break;

                    case LeaderboardType.WeeklyHours:
                        top5 = context.Users.OrderByDescending(x => x.MinutesWatchedThisWeek).Take(5).ToDictionary(x => x.Username, x => (long)x.MinutesWatchedThisWeek);
                        break;

                    case LeaderboardType.MonthlyHours:
                        top5 = context.Users.OrderByDescending(x => x.BitsDonatedThisMonth).Take(5).ToDictionary(x => x.Username, x => (long)x.BitsDonatedThisMonth);
                        break;

                    case LeaderboardType.AllTimeHours:
                        top5 = context.Users.OrderByDescending(x => x.MinutesInStream).Take(5).ToDictionary(x => x.Username, x => (long)x.MinutesInStream);
                        break;

                    default:
                        break;
                }

                var usersAndHoursSb = new StringBuilder();

                var position = 1;

                foreach (var user in top5)
                {
                    var username = $"#{position} - {user.Key} - {user.Value:N0} | ";
                    usersAndHoursSb.Append(username);
                    position++;
                }

                return usersAndHoursSb.ToString();
            }
        }
    }
}