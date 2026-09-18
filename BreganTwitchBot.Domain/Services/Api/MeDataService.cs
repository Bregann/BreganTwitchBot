using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Api
{
    /// <summary>
    /// A signed in viewer's own stats.
    ///
    /// Everything is looked up by the Twitch id from their token, which is the same
    /// id the bot stores on ChannelUser, so no account linking is involved.
    /// </summary>
    public class MeDataService(AppDbContext context) : IMeDataService
    {
        public async Task<GetMyStatsResponse?> GetMyStatsAsync(string twitchUserId)
        {
            var user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUserId == twitchUserId);

            if (user == null)
            {
                return null;
            }

            // a user only appears in a channel once the bot has seen them chat there
            var channelData = await context.ChannelUserData
                .Where(x => x.ChannelUserId == user.Id)
                .Include(x => x.Channel)
                .ToListAsync();

            var channels = new List<MyChannelStatsResponse>();

            foreach (var data in channelData)
            {
                var channel = data.Channel;

                var watchtime = await context.ChannelUserWatchtime
                    .FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

                var stats = await context.ChannelUserStats
                    .FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

                var gambleStats = await context.ChannelUserGambleStats
                    .FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

                var dailyPoints = await context.TwitchDailyPoints
                    .FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id && x.PointsClaimType == PointsClaimType.Daily);

                var discordStats = await context.DiscordUserStats
                    .FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

                var (currentRank, nextRank, minutesUntilNextRank) = await GetRankProgress(user.Id, channel.Id, watchtime?.MinutesInStream ?? 0);

                channels.Add(new MyChannelStatsResponse
                {
                    BroadcasterChannelName = channel.BroadcasterTwitchChannelName,
                    PointsName = channel.ChannelConfig.ChannelCurrencyName,
                    Points = data.Points,
                    MinutesInStream = watchtime?.MinutesInStream ?? 0,
                    MinutesWatchedThisWeek = watchtime?.MinutesWatchedThisWeek ?? 0,
                    MinutesWatchedThisMonth = watchtime?.MinutesWatchedThisMonth ?? 0,
                    TotalMessages = stats?.TotalMessages ?? 0,
                    MarblesWins = stats?.MarblesWins ?? 0,
                    CurrentRank = currentRank,
                    NextRank = nextRank,
                    MinutesUntilNextRank = minutesUntilNextRank,
                    CurrentDailyStreak = dailyPoints?.CurrentStreak ?? 0,
                    HighestDailyStreak = dailyPoints?.HighestStreak ?? 0,
                    PointsWon = gambleStats?.PointsWon ?? 0,
                    PointsLost = gambleStats?.PointsLost ?? 0,
                    DiscordLevel = discordStats?.DiscordLevel,
                    DiscordXp = discordStats?.DiscordXp
                });
            }

            return new GetMyStatsResponse
            {
                TwitchUsername = user.TwitchUsername,
                Channels = channels.OrderByDescending(x => x.MinutesInStream).ToList()
            };
        }

        public async Task<GetMySettingsResponse> GetMySettingsAsync(string twitchUserId)
        {
            var user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUserId == twitchUserId);

            if (user == null)
            {
                return new GetMySettingsResponse { Channels = [] };
            }

            var settings = await context.DiscordUserStats
                .Where(x => x.ChannelUserId == user.Id)
                .Include(x => x.Channel)
                .Select(x => new MyChannelSettingResponse
                {
                    BroadcasterChannelName = x.Channel.BroadcasterTwitchChannelName,
                    DiscordLevelUpNotifsEnabled = x.DiscordLevelUpNotifsEnabled
                })
                .ToListAsync();

            return new GetMySettingsResponse
            {
                Channels = settings.OrderBy(x => x.BroadcasterChannelName).ToList()
            };
        }

        public async Task UpdateMySettingAsync(string twitchUserId, UpdateMySettingRequest request)
        {
            var user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUserId == twitchUserId);

            if (user == null)
            {
                throw new KeyNotFoundException("The bot has not seen you in any channels yet");
            }

            // scoped to the caller's own row, so this cannot change anybody else's settings
            var stats = await context.DiscordUserStats
                .FirstOrDefaultAsync(x => x.ChannelUserId == user.Id &&
                                          x.Channel.BroadcasterTwitchChannelName.ToLower() == request.BroadcasterChannelName.ToLower());

            if (stats == null)
            {
                throw new KeyNotFoundException("You do not have any Discord stats in that channel");
            }

            stats.DiscordLevelUpNotifsEnabled = request.DiscordLevelUpNotifsEnabled;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// The rank they're on and how far off the next one is, based on watchtime
        /// </summary>
        private async Task<(string? Current, string? Next, int? MinutesUntilNext)> GetRankProgress(int channelUserId, int channelId, int minutesWatched)
        {
            var ranks = await context.ChannelRanks
                .Where(x => x.ChannelId == channelId)
                .OrderBy(x => x.RankMinutesRequired)
                .ToListAsync();

            if (ranks.Count == 0)
            {
                return (null, null, null);
            }

            var achievedRankIds = await context.ChannelUserRankProgress
                .Where(x => x.ChannelUserId == channelUserId && x.ChannelId == channelId)
                .Select(x => x.ChannelRankId)
                .ToListAsync();

            var current = ranks.LastOrDefault(x => achievedRankIds.Contains(x.Id));
            var next = ranks.FirstOrDefault(x => x.RankMinutesRequired > minutesWatched);

            return (
                current?.RankName,
                next?.RankName,
                next == null ? null : Math.Max(0, next.RankMinutesRequired - minutesWatched)
            );
        }
    }
}
