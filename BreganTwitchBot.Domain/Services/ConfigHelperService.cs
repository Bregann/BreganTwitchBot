using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Helpers;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services
{
    public class ConfigHelperService : IConfigHelperService
    {
        internal ConcurrentDictionary<string, ChannelConfig> _channelConfigs = new();
        private IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, object> _channelLocks = new();
        private readonly ConcurrentDictionary<ulong, object> _discordGuildLocks = new();

        public ConfigHelperService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var configs = context.ChannelConfig.Include(c => c.Channel).ToList();

                foreach (var config in configs)
                {
                    _channelConfigs[config.Channel.BroadcasterTwitchChannelId] = config;
                    Log.Information($"Loaded config for channel {config.Channel.BroadcasterTwitchChannelName} from database");
                }
            }
        }

        public async Task UpdateDailyPointsStatus(string broadcasterId, bool status)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (status)
                {
                    await context.ChannelConfig
                        .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.DailyPointsCollectingAllowed, status)
                            .SetProperty(x => x.LastDailyPointsAllowed, DateTime.UtcNow)
                    );

                }
                else
                {
                    await context.ChannelConfig
                        .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.DailyPointsCollectingAllowed, status)
                    );
                }

                lock (GetLock(broadcasterId))
                {
                    if (_channelConfigs.TryGetValue(broadcasterId, out var config))
                    {
                        config.DailyPointsCollectingAllowed = status;

                        if (status)
                        {
                            config.LastDailyPointsAllowed = DateTime.UtcNow;
                        }
                    }
                }

                Log.Information($"Updated daily points status for {broadcasterId} to {status}");
            }
        }

        public async Task UpdateStreamLiveStatus(string broadcasterId, bool status)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.UtcNow;

                await context.ChannelConfig
                    .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.StreamAnnounced, status)
                        .SetProperty(x => status == true ? x.LastStreamStartDate : x.LastStreamEndDate, now)
                        .SetProperty(x => x.StreamHappenedThisWeek, true)
                        .SetProperty(x => x.BroadcasterLive, status)
                );

                lock (GetLock(broadcasterId))
                {
                    if (_channelConfigs.TryGetValue(broadcasterId, out var config))
                    {
                        config.StreamAnnounced = status;
                        config.BroadcasterLive = status;
                        config.StreamHappenedThisWeek = true;

                        if (status)
                        {
                            config.LastStreamStartDate = now;
                            Log.Information($"Stream for {broadcasterId} is now LIVE! Started at {now}");
                        }
                        else
                        {
                            config.LastStreamEndDate = now;
                            Log.Information($"Stream for {broadcasterId} ended at {now}");
                        }
                    }
                }

                Log.Information($"Updated stream live status for {broadcasterId} to {status}");
            }
        }

        public (bool DailyPointsAllowed, DateTime LastStreamDate, DateTime LastDailyPointedAllowedDate, bool StreamHappenedThisWeek) GetDailyPointsStatus(string broadcasterId)
        {
            lock (GetLock(broadcasterId))
            {
                if (_channelConfigs.TryGetValue(broadcasterId, out var config))
                {
                    return (
                        DailyPointsAllowed: config.DailyPointsCollectingAllowed,
                        LastStreamDate: config.LastStreamStartDate,
                        LastDailyPointedAllowedDate: config.LastDailyPointsAllowed,
                        StreamHappenedThisWeek: config.StreamHappenedThisWeek
                    );
                }

                throw new KeyNotFoundException($"No config found for broadcasterId {broadcasterId}");
            }
        }

        public DiscordConfig? GetDiscordConfig(ulong discordGuildId)
        {
            lock (GetDiscordLock(discordGuildId))
            {
                var channelConfigEntry = _channelConfigs.FirstOrDefault(x => x.Value.DiscordGuildId == discordGuildId);

                if (channelConfigEntry.Value == null)
                {
                    Log.Warning($"No Discord config found for guild ID {discordGuildId}");
                    return null;
                }

                var config = channelConfigEntry.Value;

                return new DiscordConfig
                {
                    DiscordGuildOwnerId = config.DiscordGuildOwnerId,
                    DiscordEventChannelId = config.DiscordEventChannelId,
                    DiscordStreamAnnouncementChannelId = config.DiscordStreamAnnouncementChannelId,
                    DiscordUserCommandsChannelId = config.DiscordUserCommandsChannelId,
                    DiscordUserRankUpAnnouncementChannelId = config.DiscordUserRankUpAnnouncementChannelId,
                    DiscordGiveawayChannelId = config.DiscordGiveawayChannelId,
                    DiscordModeratorRoleId = config.DiscordModeratorRoleId,
                    DiscordWelcomeMessageChannelId = config.DiscordWelcomeMessageChannelId,
                    DiscordGeneralChannelId = config.DiscordGeneralChannelId,
                    DiscordGuildId = config.DiscordGuildId
                };
            }
        }

        public DiscordConfig? GetDiscordConfig(string broadcasterId)
        {
            lock (GetLock(broadcasterId))
            {
                var channelConfigEntry = _channelConfigs.TryGetValue(broadcasterId, out var config);

                if (config == null)
                {
                    Log.Warning($"No Discord config found for broadcaster id {broadcasterId}");
                    return null;
                }

                return new DiscordConfig
                {
                    DiscordGuildOwnerId = config.DiscordGuildOwnerId,
                    DiscordEventChannelId = config.DiscordEventChannelId,
                    DiscordStreamAnnouncementChannelId = config.DiscordStreamAnnouncementChannelId,
                    DiscordUserCommandsChannelId = config.DiscordUserCommandsChannelId,
                    DiscordUserRankUpAnnouncementChannelId = config.DiscordUserRankUpAnnouncementChannelId,
                    DiscordGiveawayChannelId = config.DiscordGiveawayChannelId,
                    DiscordModeratorRoleId = config.DiscordModeratorRoleId,
                    DiscordWelcomeMessageChannelId = config.DiscordWelcomeMessageChannelId,
                    DiscordGeneralChannelId = config.DiscordGeneralChannelId,
                    DiscordGuildId = config.DiscordGuildId
                };
            }
        }

        public bool IsDiscordEnabled(string broadcasterId)
        {
            lock (GetLock(broadcasterId))
            {
                if (!_channelConfigs.TryGetValue(broadcasterId, out var config))
                {
                    Log.Warning($"No config found for broadcasterId {broadcasterId}");
                    return false;
                }

                return config.DiscordEnabled;
            }
        }

        private object GetLock(string broadcasterId)
        {
            return _channelLocks.GetOrAdd(broadcasterId, _ => new object());
        }

        private object GetDiscordLock(ulong discordGuildId)
        {
            return _discordGuildLocks.GetOrAdd(discordGuildId, _ => new object());
        }
    }
}
