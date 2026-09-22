using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Enums;
using Discord;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services.Twitch
{
    /// <summary>
    /// Per stream counters.
    ///
    /// Counts are accumulated in memory and flushed periodically rather than written per event,
    /// as some of these tick on every chat message. The old bot held a single static counter set,
    /// which cannot work now one bot serves many channels, so everything here is keyed by
    /// broadcaster channel id.
    /// </summary>
    public class StreamStatsService(IServiceProvider serviceProvider, ITwitchApiConnection twitchApiConnection, ITwitchApiInteractionService twitchApiInteractionService, IDiscordHelperService discordHelperService, IConfigHelperService configHelperService) : IStreamStatsService
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<StreamStatType, long>> _pendingStats = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _uniqueViewers = new();

        /// <summary>
        /// Last known follower count per channel, so the hourly check can report the change
        /// </summary>
        private readonly ConcurrentDictionary<string, long> _lastFollowerCounts = new();

        public void UpdateStreamStat(string broadcasterChannelId, StreamStatType statType, long amount = 1)
        {
            var channelStats = _pendingStats.GetOrAdd(broadcasterChannelId, _ => new ConcurrentDictionary<StreamStatType, long>());
            channelStats.AddOrUpdate(statType, amount, (_, existing) => existing + amount);
        }

        public void AddUniqueViewer(string broadcasterChannelId, string username)
        {
            var viewers = _uniqueViewers.GetOrAdd(broadcasterChannelId, _ => new ConcurrentDictionary<string, byte>());
            viewers.TryAdd(username.ToLower(), 0);
        }

        public async Task FlushStats()
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var broadcasterChannelId in _pendingStats.Keys.ToArray())
            {
                // take the pending counts out of the dictionary so events arriving during the
                // write land in a fresh bucket rather than being lost
                if (!_pendingStats.TryRemove(broadcasterChannelId, out var channelStats) || channelStats.IsEmpty)
                {
                    continue;
                }

                try
                {
                    var stream = await GetCurrentStream(context, broadcasterChannelId);

                    if (stream == null)
                    {
                        continue;
                    }

                    foreach (var (statType, amount) in channelStats)
                    {
                        ApplyStat(stream, statType, amount);
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Stream Stats] Error flushing stats for {broadcasterChannelId}");
                }
            }

            await FlushUniqueViewers(context);
        }

        public async Task StartNewStream(string broadcasterChannelId)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelId == broadcasterChannelId);

            if (channel == null)
            {
                Log.Error($"[Stream Stats] Cannot start a stream for unknown channel {broadcasterChannelId}");
                return;
            }

            var (followerCount, subCount) = await GetFollowerAndSubCounts(channel.BroadcasterTwitchChannelName, broadcasterChannelId);

            var lastStreamId = await context.TwitchStreamStats
                .Where(x => x.ChannelId == channel.Id)
                .OrderByDescending(x => x.StreamId)
                .Select(x => (long?)x.StreamId)
                .FirstOrDefaultAsync() ?? 0;

            await context.TwitchStreamStats.AddAsync(new TwitchStreamStats
            {
                ChannelId = channel.Id,
                StreamId = lastStreamId + 1,
                StreamStarted = DateTime.UtcNow,
                StartingFollowerCount = followerCount,
                StartingSubscriberCount = subCount,
                EndingFollowerCount = 0,
                EndingSubscriberCount = 0,
                AvgViewCount = 0,
                PeakViewerCount = 0,
                BitsDonated = 0,
                CommandsSent = 0,
                DiscordRanksEarnt = 0,
                MessagesReceived = 0,
                NewFollowers = 0,
                NewSubscribers = 0,
                NewGiftedSubs = 0,
                PointsGainedSubscribing = 0,
                PointsGainedWatching = 0,
                PointsGambled = 0,
                PointsLost = 0,
                PointsWon = 0,
                GiftedPoints = 0,
                SongRequestsBlacklisted = 0,
                SongRequestsLiked = 0,
                SongRequestsSent = 0,
                TotalBans = 0,
                TotalTimeouts = 0,
                TotalSpins = 0,
                KappaWins = 0,
                ForeheadWins = 0,
                LulWins = 0,
                SmorcWins = 0,
                JackpotWins = 0,
                TotalUsersClaimed = 0,
                TotalPointsClaimed = 0,
                AmountOfUsersReset = 0,
                AmountOfRewardsRedeemed = 0,
                RewardRedeemCost = 0,
                AmountOfDiscordUsersJoined = 0,
                UniquePeople = 0
            });

            // the previous stream's viewer samples and unique viewers belong to that stream
            var oldViewCounts = context.StreamViewCounts.Where(x => x.ChannelId == channel.Id);
            context.StreamViewCounts.RemoveRange(oldViewCounts);

            var oldUniqueViewers = context.UniqueViewers.Where(x => x.ChannelId == channel.Id);
            context.UniqueViewers.RemoveRange(oldUniqueViewers);

            await context.SaveChangesAsync();

            _pendingStats.TryRemove(broadcasterChannelId, out _);
            _uniqueViewers.TryRemove(broadcasterChannelId, out _);

            Log.Information($"[Stream Stats] Started stream {lastStreamId + 1} for {channel.BroadcasterTwitchChannelName}");
        }

        public async Task EndStream(string broadcasterChannelId)
        {
            await FlushStats();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelId == broadcasterChannelId);

            if (channel == null)
            {
                return;
            }

            var stream = await GetCurrentStream(context, broadcasterChannelId);

            if (stream == null)
            {
                return;
            }

            var (followerCount, subCount) = await GetFollowerAndSubCounts(channel.BroadcasterTwitchChannelName, broadcasterChannelId);

            stream.EndingFollowerCount = followerCount;
            stream.EndingSubscriberCount = subCount;
            stream.StreamEnded = DateTime.UtcNow;
            stream.Uptime = DateTime.UtcNow - stream.StreamStarted;

            var viewCounts = await context.StreamViewCounts.Where(x => x.ChannelId == channel.Id).Select(x => x.ViewCount).ToListAsync();

            if (viewCounts.Count > 0)
            {
                stream.AvgViewCount = Math.Round(viewCounts.Average(), 2);
                stream.PeakViewerCount = viewCounts.Max();
            }

            stream.UniquePeople = await context.UniqueViewers.CountAsync(x => x.ChannelId == channel.Id);

            await context.SaveChangesAsync();

            Log.Information($"[Stream Stats] Ended stream {stream.StreamId} for {channel.BroadcasterTwitchChannelName}");
        }

        public async Task RecordViewerCount(string broadcasterChannelId, int viewerCount)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelId == broadcasterChannelId);

            if (channel == null)
            {
                return;
            }

            await context.StreamViewCounts.AddAsync(new StreamViewCount
            {
                ChannelId = channel.Id,
                Time = DateTime.UtcNow,
                ViewCount = viewerCount
            });

            await context.SaveChangesAsync();
        }

        public async Task SampleViewerCounts()
        {
            foreach (var channel in twitchApiConnection.GetAllChannels())
            {
                try
                {
                    var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(channel.BroadcasterChannelName);

                    if (apiClient == null)
                    {
                        continue;
                    }

                    var stream = await twitchApiInteractionService.GetStreams(apiClient.ApiClient, channel.BroadcasterChannelId);

                    // not live, nothing to sample
                    if (stream == null)
                    {
                        continue;
                    }

                    await RecordViewerCount(channel.BroadcasterChannelId, stream.ViewerCount);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Stream Stats] Error sampling the viewer count for {channel.BroadcasterChannelName}");
                }
            }
        }

        public async Task ReportFollowerChanges()
        {
            foreach (var channel in twitchApiConnection.GetAllChannels())
            {
                try
                {
                    var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(channel.BroadcasterChannelName);

                    if (apiClient == null)
                    {
                        continue;
                    }

                    var followerCount = await twitchApiInteractionService.GetChannelFollowerCount(apiClient.ApiClient, channel.BroadcasterChannelId, apiClient.TwitchChannelClientId);

                    // the first run of a session just records the baseline, as reporting a change
                    // against zero would claim every follower was gained in the last hour
                    if (!_lastFollowerCounts.TryGetValue(channel.BroadcasterChannelId, out var previousCount))
                    {
                        _lastFollowerCounts[channel.BroadcasterChannelId] = followerCount;
                        continue;
                    }

                    _lastFollowerCounts[channel.BroadcasterChannelId] = followerCount;

                    if (followerCount == previousCount)
                    {
                        Log.Information($"[Follower Check] No change for {channel.BroadcasterChannelName}, still {followerCount:N0}");
                        continue;
                    }

                    if (!configHelperService.IsDiscordEnabled(channel.BroadcasterChannelId))
                    {
                        continue;
                    }

                    var discordConfig = configHelperService.GetDiscordConfig(channel.BroadcasterChannelId);

                    if (discordConfig?.DiscordEventChannelId == null)
                    {
                        continue;
                    }

                    var change = followerCount - previousCount;

                    var embed = new EmbedBuilder
                    {
                        Title = "Follow count",
                        Timestamp = DateTime.Now,
                        Color = new Color(0, 217, 22)
                    };

                    embed.AddField("Before", $"{previousCount:N0}", true);
                    embed.AddField("Now", $"{followerCount:N0}", true);
                    embed.AddField("Change", $"{(change > 0 ? "+" : "")}{change:N0}", true);

                    await discordHelperService.SendEmbedMessage(discordConfig.DiscordEventChannelId.Value, embed);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Follower Check] Error checking followers for {channel.BroadcasterChannelName}");
                }
            }
        }

        /// <summary>
        /// The most recent stream row for a channel
        /// </summary>
        private static async Task<TwitchStreamStats?> GetCurrentStream(AppDbContext context, string broadcasterChannelId)
        {
            return await context.TwitchStreamStats
                .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId)
                .OrderByDescending(x => x.StreamId)
                .FirstOrDefaultAsync();
        }

        private async Task FlushUniqueViewers(AppDbContext context)
        {
            foreach (var broadcasterChannelId in _uniqueViewers.Keys.ToArray())
            {
                if (!_uniqueViewers.TryGetValue(broadcasterChannelId, out var viewers) || viewers.IsEmpty)
                {
                    continue;
                }

                try
                {
                    var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelId == broadcasterChannelId);

                    if (channel == null)
                    {
                        continue;
                    }

                    var alreadyStored = await context.UniqueViewers
                        .Where(x => x.ChannelId == channel.Id)
                        .Select(x => x.Username)
                        .ToListAsync();

                    var toAdd = viewers.Keys.Except(alreadyStored).ToList();

                    if (toAdd.Count == 0)
                    {
                        continue;
                    }

                    await context.UniqueViewers.AddRangeAsync(toAdd.Select(username => new UniqueViewers
                    {
                        ChannelId = channel.Id,
                        Username = username
                    }));

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Stream Stats] Error flushing unique viewers for {broadcasterChannelId}");
                }
            }
        }

        private async Task<(long FollowerCount, int SubCount)> GetFollowerAndSubCounts(string broadcasterChannelName, string broadcasterChannelId)
        {
            long followerCount = 0;
            var subCount = 0;

            // these need the channel's own broadcaster token
            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(broadcasterChannelName);

            if (apiClient == null)
            {
                Log.Warning($"[Stream Stats] No broadcaster api client for {broadcasterChannelName}, counts will be 0");
                return (followerCount, subCount);
            }

            try
            {
                followerCount = await twitchApiInteractionService.GetChannelFollowerCount(apiClient.ApiClient, broadcasterChannelId, apiClient.TwitchChannelClientId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[Stream Stats] Error getting the follower count for {broadcasterChannelName}");
            }

            try
            {
                subCount = await twitchApiInteractionService.GetChannelSubscriberCount(apiClient.ApiClient, broadcasterChannelId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[Stream Stats] Error getting the sub count for {broadcasterChannelName}");
            }

            return (followerCount, subCount);
        }

        private static void ApplyStat(TwitchStreamStats stream, StreamStatType statType, long amount)
        {
            switch (statType)
            {
                case StreamStatType.BitsDonated: stream.BitsDonated += amount; break;
                case StreamStatType.CommandsSent: stream.CommandsSent += amount; break;
                case StreamStatType.DiscordRanksEarnt: stream.DiscordRanksEarnt += amount; break;
                case StreamStatType.MessagesReceived: stream.MessagesReceived += amount; break;
                case StreamStatType.NewFollowers: stream.NewFollowers += amount; break;
                case StreamStatType.NewSubscribers: stream.NewSubscribers += amount; break;
                case StreamStatType.NewGiftedSubs: stream.NewGiftedSubs += amount; break;
                case StreamStatType.PointsGainedSubscribing: stream.PointsGainedSubscribing += amount; break;
                case StreamStatType.PointsGainedWatching: stream.PointsGainedWatching += amount; break;
                case StreamStatType.PointsGambled: stream.PointsGambled += amount; break;
                case StreamStatType.PointsLost: stream.PointsLost += amount; break;
                case StreamStatType.PointsWon: stream.PointsWon += amount; break;
                case StreamStatType.GiftedPoints: stream.GiftedPoints += amount; break;
                case StreamStatType.TotalBans: stream.TotalBans += amount; break;
                case StreamStatType.TotalTimeouts: stream.TotalTimeouts += amount; break;
                case StreamStatType.TotalSpins: stream.TotalSpins += amount; break;
                case StreamStatType.KappaWins: stream.KappaWins += amount; break;
                case StreamStatType.ForeheadWins: stream.ForeheadWins += amount; break;
                case StreamStatType.LulWins: stream.LulWins += amount; break;
                case StreamStatType.SmorcWins: stream.SmorcWins += amount; break;
                case StreamStatType.JackpotWins: stream.JackpotWins += amount; break;
                case StreamStatType.TotalUsersClaimed: stream.TotalUsersClaimed += amount; break;
                case StreamStatType.TotalPointsClaimed: stream.TotalPointsClaimed += amount; break;
                case StreamStatType.AmountOfUsersReset: stream.AmountOfUsersReset += amount; break;
                case StreamStatType.AmountOfRewardsRedeemed: stream.AmountOfRewardsRedeemed += amount; break;
                case StreamStatType.RewardRedeemCost: stream.RewardRedeemCost += amount; break;
                case StreamStatType.AmountOfDiscordUsersJoined: stream.AmountOfDiscordUsersJoined += amount; break;
                case StreamStatType.SongRequestsSent: stream.SongRequestsSent += amount; break;
                case StreamStatType.SongRequestsLiked: stream.SongRequestsLiked += amount; break;
                case StreamStatType.SongRequestsBlacklisted: stream.SongRequestsBlacklisted += amount; break;
            }
        }
    }
}
