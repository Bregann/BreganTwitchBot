using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Api;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Api
{
    /// <summary>
    /// Read models for the website. Everything is addressed by broadcaster channel name, as the
    /// old single channel api had no way of saying which channel it meant.
    /// </summary>
    public class ApiDataService(AppDbContext context, IDiscordLeaderboardsData discordLeaderboardsData) : IApiDataService
    {
        public async Task<GetLeaderboardResponse?> GetLeaderboardAsync(string broadcasterChannelName, DiscordLeaderboardType type, int take = 250)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return null;
            }

            var guildId = channel.ChannelConfig.DiscordGuildId;

            // the leaderboard queries are shared with the discord side, keyed by guild
            if (guildId == null)
            {
                return new GetLeaderboardResponse
                {
                    LeaderboardName = type.ToString(),
                    BroadcasterChannelName = channel.BroadcasterTwitchChannelName,
                    Positions = []
                };
            }

            var entries = await discordLeaderboardsData.GetLeaderboard(guildId.Value, type, take);

            return new GetLeaderboardResponse
            {
                LeaderboardName = type.ToString(),
                BroadcasterChannelName = channel.BroadcasterTwitchChannelName,
                Positions = entries.Select(x => new LeaderboardPositionResponse
                {
                    Position = x.Position,
                    Username = x.Username,
                    Value = x.Value
                }).ToList()
            };
        }

        public async Task<List<GetCustomCommandResponse>?> GetCustomCommandsAsync(string broadcasterChannelName)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return null;
            }

            return await context.CustomCommands
                .Where(x => x.ChannelId == channel.Id)
                .OrderBy(x => x.CommandName)
                .Select(x => new GetCustomCommandResponse
                {
                    CommandName = x.CommandName,
                    CommandText = x.CommandText,
                    TimesUsed = x.TimesUsed
                })
                .ToListAsync();
        }

        public async Task<GetSubathonStatusResponse?> GetSubathonStatusAsync(string broadcasterChannelName)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return null;
            }

            var config = channel.ChannelConfig;
            var endsAt = config.SubathonStartTime?.Add(config.SubathonTime);
            var secondsLeft = 0;

            if (endsAt != null)
            {
                var timeLeft = endsAt.Value - DateTime.UtcNow;
                secondsLeft = timeLeft > TimeSpan.Zero ? (int)Math.Round(timeLeft.TotalSeconds) : 0;
            }

            return new GetSubathonStatusResponse
            {
                Active = config.SubathonActive,
                SecondsLeft = secondsLeft,
                TotalTimeAdded = DurationFormatHelper.Humanise(config.SubathonTime),
                StartedAt = config.SubathonStartTime,
                EndsAt = endsAt
            };
        }

        public async Task<GetChannelSummaryResponse?> GetChannelSummaryAsync(string broadcasterChannelName)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return null;
            }

            return new GetChannelSummaryResponse
            {
                BroadcasterChannelName = channel.BroadcasterTwitchChannelName,
                IsLive = channel.ChannelConfig.BroadcasterLive,
                PointsName = channel.ChannelConfig.ChannelCurrencyName,
                SubathonActive = channel.ChannelConfig.SubathonActive,
                TrackedViewers = await context.ChannelUserData.CountAsync(x => x.ChannelId == channel.Id),
                TotalStreams = await context.TwitchStreamStats.CountAsync(x => x.ChannelId == channel.Id)
            };
        }

        public async Task<List<GetStreamHistoryResponse>?> GetStreamHistoryAsync(string broadcasterChannelName, int take = 30)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return null;
            }

            var streams = await context.TwitchStreamStats
                .Where(x => x.ChannelId == channel.Id)
                .OrderByDescending(x => x.StreamId)
                .Take(take)
                .ToListAsync();

            return streams.Select(x => new GetStreamHistoryResponse
            {
                StreamId = x.StreamId,
                StreamStarted = x.StreamStarted,
                StreamEnded = x.StreamEnded == default ? null : x.StreamEnded,
                AvgViewCount = x.AvgViewCount,
                PeakViewerCount = x.PeakViewerCount,
                MessagesReceived = x.MessagesReceived,
                NewFollowers = x.NewFollowers,
                NewSubscribers = x.NewSubscribers,
                BitsDonated = x.BitsDonated,
                UniquePeople = x.UniquePeople,
                Uptime = DurationFormatHelper.Humanise(x.Uptime)
            }).ToList();
        }

        public async Task<GetSubathonLeaderboardResponse?> GetSubathonLeaderboardAsync(string broadcasterChannelName, int take = 10)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return null;
            }

            var topBits = await context.Subathons
                .Where(x => x.ChannelId == channel.Id && x.BitsDonated > 0)
                .OrderByDescending(x => x.BitsDonated)
                .Take(take)
                .Select(x => new { x.ChannelUser.TwitchUsername, Amount = x.BitsDonated })
                .ToListAsync();

            var topSubs = await context.Subathons
                .Where(x => x.ChannelId == channel.Id && x.SubsGifted > 0)
                .OrderByDescending(x => x.SubsGifted)
                .Take(take)
                .Select(x => new { x.ChannelUser.TwitchUsername, Amount = (long)x.SubsGifted })
                .ToListAsync();

            return new GetSubathonLeaderboardResponse
            {
                TopBitsDonators = topBits.Select((x, i) => new SubathonContributorResponse
                {
                    Position = i + 1,
                    Username = x.TwitchUsername,
                    Amount = x.Amount
                }).ToList(),
                TopSubGifters = topSubs.Select((x, i) => new SubathonContributorResponse
                {
                    Position = i + 1,
                    Username = x.TwitchUsername,
                    Amount = x.Amount
                }).ToList()
            };
        }
    }
}
