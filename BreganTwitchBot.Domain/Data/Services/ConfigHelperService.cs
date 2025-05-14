using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Helpers;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services
{
    public class ConfigHelperService : IConfigHelperService
    {
        public List<ChannelConfig> _channelConfigs = new();
        private IServiceProvider _serviceProvider;

        public ConfigHelperService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _channelConfigs = context.ChannelConfig.Include(c => c.Channel).ToList();
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

                    _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).LastDailyPointsAllowed = DateTime.UtcNow;
                }
                else
                {
                    await context.ChannelConfig
                        .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.DailyPointsCollectingAllowed, status)
                    );
                }

                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).DailyPointsCollectingAllowed = status;
                Log.Information($"Updated daily points status for {broadcasterId} to {status}");
            }
        }

        public async Task UpdateStreamLiveStatus(string broadcasterId, bool status)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await context.ChannelConfig
                    .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.StreamAnnounced, status)
                        .SetProperty(x => status == true ? x.LastStreamStartDate : x.LastStreamEndDate, DateTime.UtcNow)
                        .SetProperty(x => x.StreamHappenedThisWeek, true)
                        .SetProperty(x => x.BroadcasterLive, status)
                        .SetProperty(x => x.DailyPointsCollectingAllowed, status)
                );

                await context.SaveChangesAsync();

                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).StreamAnnounced = status;
                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).BroadcasterLive = status;
                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).DailyPointsCollectingAllowed = status;

                if (status)
                {
                    _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).LastStreamStartDate = DateTime.UtcNow;
                }
                else
                {
                    _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).LastStreamEndDate = DateTime.UtcNow;
                }

                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).StreamHappenedThisWeek = true;
                Log.Information($"Updated stream live status for {broadcasterId} to {status}");
            }
        }

        public (bool DailyPointsAllowed, DateTime LastStreamDate, DateTime LastDailyPointedAllowedDate, bool StreamHappenedThisWeek) GetDailyPointsStatus(string broadcasterId)
        {
            var config = _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId);
            return (DailyPointsAllowed: config.DailyPointsCollectingAllowed, LastStreamDate: config.LastStreamStartDate, LastDailyPointedAllowedDate: config.LastDailyPointsAllowed, config.StreamHappenedThisWeek);
        }

        public DiscordConfig? GetDiscordConfig(ulong discordGuildId)
        {
            var config = _channelConfigs.Where(x => x.DiscordGuildId == discordGuildId).FirstOrDefault();

            return config == null
                ? null
                : new DiscordConfig
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

        public DiscordConfig? GetDiscordConfig(string broadcasterId)
        {
            var config = _channelConfigs.FirstOrDefault(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId);
            return config == null
                ? null
                : new DiscordConfig
                {
                    DiscordGuildOwnerId = config.DiscordGuildOwnerId,
                    DiscordEventChannelId = config.DiscordEventChannelId,
                    DiscordStreamAnnouncementChannelId = config.DiscordStreamAnnouncementChannelId,
                    DiscordUserCommandsChannelId = config.DiscordUserCommandsChannelId,
                    DiscordUserRankUpAnnouncementChannelId = config.DiscordUserRankUpAnnouncementChannelId,
                    DiscordGiveawayChannelId = config.DiscordGiveawayChannelId,
                    DiscordModeratorRoleId = config.DiscordModeratorRoleId,
                    DiscordWelcomeMessageChannelId = config.DiscordWelcomeMessageChannelId,
                    DiscordGeneralChannelId = config.DiscordGeneralChannelId
                };
        }

        public bool IsDiscordEnabled(string broadcasterId)
        {
            var config = _channelConfigs.FirstOrDefault(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId);

            return config != null && config.DiscordEnabled;
        }
    }
}
