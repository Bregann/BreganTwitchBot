using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Leaderboards
{
    public class LeaderboardsDataService(AppDbContext context) : ILeaderboardsDataService
    {
        public async Task<string> HandleLeaderboardCommand(ChannelChatMessageReceivedParams msgParams, LeaderboardType type)
        {
            var top5 = new Dictionary<string, long>();

            switch (type)
            {
                case LeaderboardType.Points:
                    top5 = await context.ChannelUserData.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.Points)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => x.Points);
                    break;
                case LeaderboardType.AllTimeHours:
                    top5 = await context.ChannelUserWatchtime.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.MinutesInStream)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => (long)x.MinutesInStream);
                    break;
                case LeaderboardType.StreamHours:
                    top5 = await context.ChannelUserWatchtime.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.MinutesInStream)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => (long)x.MinutesWatchedThisStream);
                    break;
                case LeaderboardType.WeeklyHours:
                    top5 = await context.ChannelUserWatchtime.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.MinutesWatchedThisWeek)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => (long)x.MinutesWatchedThisWeek);
                    break;
                case LeaderboardType.MonthlyHours:
                    top5 = await context.ChannelUserWatchtime.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.MinutesWatchedThisMonth)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => (long)x.MinutesWatchedThisMonth);
                    break;
                case LeaderboardType.PointsWon:
                    top5 = await context.ChannelUserGambleStats.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.PointsWon)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => x.PointsWon);
                    break;
                case LeaderboardType.PointsLost:
                    top5 = await context.ChannelUserGambleStats.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.PointsLost)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => x.PointsLost);
                    break;
                case LeaderboardType.PointsGambled:
                    top5 = await context.ChannelUserGambleStats.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.PointsGambled)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => x.PointsGambled);
                    break;
                case LeaderboardType.TotalSpins:
                    top5 = await context.ChannelUserGambleStats.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.TotalSpins)
                        .Take(5)
                        .ToDictionaryAsync(x => x.ChannelUser.TwitchUsername, x => (long)x.TotalSpins);
                    break;
                case LeaderboardType.CurrentDailyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Daily)
                        .OrderByDescending(x => x.CurrentStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.CurrentStreak);
                    break;
                case LeaderboardType.HighestDailyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Daily)
                        .OrderByDescending(x => x.HighestStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.HighestStreak);
                    break;
                case LeaderboardType.CurrentWeeklyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Weekly)
                        .OrderByDescending(x => x.CurrentStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.CurrentStreak);
                    break;
                case LeaderboardType.HighestWeeklyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Weekly)
                        .OrderByDescending(x => x.HighestStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.HighestStreak);
                    break;
                case LeaderboardType.CurrentMonthlyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Monthly)
                        .OrderByDescending(x => x.CurrentStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.CurrentStreak);
                    break;
                case LeaderboardType.HighestMonthlyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Monthly)
                        .OrderByDescending(x => x.HighestStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.HighestStreak);
                    break;
                case LeaderboardType.CurrentYearlyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Yearly)
                        .OrderByDescending(x => x.CurrentStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.CurrentStreak);
                    break;
                case LeaderboardType.HighestYearlyStreak:
                    top5 = await context.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.PointsClaimType == PointsClaimType.Yearly)
                        .OrderByDescending(x => x.HighestStreak)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.HighestStreak);
                    break;
                case LeaderboardType.BossesDone:
                    top5 = await context.ChannelUserStats.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.BossesDone)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => (long)x.BossesDone);
                    break;
                case LeaderboardType.BossesPointsWon:
                    top5 = await context.ChannelUserStats.Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)
                        .OrderByDescending(x => x.BossesPointsWon)
                        .Take(5)
                        .ToDictionaryAsync(x => x.User.TwitchUsername, x => x.BossesPointsWon);
                    break;
                default:
                    break;
            }


            var usersandItemSb = new StringBuilder();
            var position = 1;

            foreach (var user in top5)
            {
                var username = $"#{position} - {user.Key} - {user.Value:N0} {(type == LeaderboardType.AllTimeHours || type == LeaderboardType.MonthlyHours || type == LeaderboardType.WeeklyHours || type == LeaderboardType.StreamHours ? "minutes " : "")}| ";
                usersandItemSb.Append(username);
                position++;
            }

            return usersandItemSb.ToString().TrimEnd('|', ' ');
        }
    }
}
