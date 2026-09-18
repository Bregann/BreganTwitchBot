using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Leaderboards
{
    public class DiscordLeaderboardsData(AppDbContext context) : IDiscordLeaderboardsData
    {
        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(ulong guildId, DiscordLeaderboardType type, int take = 24)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);

            if (channel == null)
            {
                return [];
            }

            var channelId = channel.Id;

            // each leaderboard is a different table, so they're projected to a common shape here
            var results = type switch
            {
                DiscordLeaderboardType.Points => await context.ChannelUserData
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.Points)
                    .Take(take)
                    .Select(x => new { x.ChannelUser.TwitchUsername, Value = x.Points })
                    .ToListAsync(),

                DiscordLeaderboardType.Marbles => await context.ChannelUserStats
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.MarblesWins)
                    .Take(take)
                    .Select(x => new { x.User.TwitchUsername, Value = (long)x.MarblesWins })
                    .ToListAsync(),

                // daily points are stored per claim type, so the daily streak is the Daily rows
                DiscordLeaderboardType.DailyStreak => await context.TwitchDailyPoints
                    .Where(x => x.ChannelId == channelId && x.PointsClaimType == PointsClaimType.Daily)
                    .OrderByDescending(x => x.CurrentStreak)
                    .Take(take)
                    .Select(x => new { x.User.TwitchUsername, Value = (long)x.CurrentStreak })
                    .ToListAsync(),

                DiscordLeaderboardType.DiscordLevel => await context.DiscordUserStats
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.DiscordLevel)
                    .Take(take)
                    .Select(x => new { x.User.TwitchUsername, Value = (long)x.DiscordLevel })
                    .ToListAsync(),

                DiscordLeaderboardType.DiscordXp => await context.DiscordUserStats
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.DiscordXp)
                    .Take(take)
                    .Select(x => new { x.User.TwitchUsername, Value = x.DiscordXp })
                    .ToListAsync(),

                DiscordLeaderboardType.StreamHours => await context.ChannelUserWatchtime
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.MinutesWatchedThisStream)
                    .Take(take)
                    .Select(x => new { x.ChannelUser.TwitchUsername, Value = (long)x.MinutesWatchedThisStream })
                    .ToListAsync(),

                DiscordLeaderboardType.WeeklyHours => await context.ChannelUserWatchtime
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.MinutesWatchedThisWeek)
                    .Take(take)
                    .Select(x => new { x.ChannelUser.TwitchUsername, Value = (long)x.MinutesWatchedThisWeek })
                    .ToListAsync(),

                // the old bot ordered this one by BitsDonatedThisMonth, which looks like a
                // copy paste slip - it's monthly watchtime
                DiscordLeaderboardType.MonthlyHours => await context.ChannelUserWatchtime
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.MinutesWatchedThisMonth)
                    .Take(take)
                    .Select(x => new { x.ChannelUser.TwitchUsername, Value = (long)x.MinutesWatchedThisMonth })
                    .ToListAsync(),

                DiscordLeaderboardType.AllTimeHours => await context.ChannelUserWatchtime
                    .Where(x => x.ChannelId == channelId)
                    .OrderByDescending(x => x.MinutesInStream)
                    .Take(take)
                    .Select(x => new { x.ChannelUser.TwitchUsername, Value = (long)x.MinutesInStream })
                    .ToListAsync(),

                _ => []
            };

            return results
                .Select((x, index) => new LeaderboardEntry
                {
                    Position = index + 1,
                    Username = x.TwitchUsername,
                    Value = x.Value
                })
                .ToList();
        }
    }
}
