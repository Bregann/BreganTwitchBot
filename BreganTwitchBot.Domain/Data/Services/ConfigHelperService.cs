using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Helpers;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services
{
    public class ConfigHelperService : IConfigHelperService
    {
        public List<ChannelConfig> _channelConfigs = new ();
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
                
                await context.ChannelConfig
                    .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.DailyPointsCollectingAllowed, status)
                        .SetProperty(x => x.LastDailyPointsAllowed, DateTime.UtcNow)
                );

                await context.SaveChangesAsync();

                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).DailyPointsCollectingAllowed = status;
                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).LastDailyPointsAllowed = DateTime.UtcNow;
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
                );

                await context.SaveChangesAsync();

                _channelConfigs.First(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId).StreamAnnounced = status;

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
            return (DailyPointsAllowed: config.DailyPointsCollectingAllowed, LastStreamDate: config.LastStreamEndDate, LastDailyPointedAllowedDate: config.LastDailyPointsAllowed, config.StreamHappenedThisWeek);
        }

        public DiscordConfig? GetDiscordConfig(ulong discordGuildId)
        {
            var config = _channelConfigs.Where(x => x.Channel.DiscordGuildId == discordGuildId).FirstOrDefault();

            return config == null
                ? null
                : new DiscordConfig
            {
                DiscordGuildOwnerId = config.DiscordGuildOwnerId,
                DiscordEventChannelId = config.DiscordEventChannelId,
                DiscordStreamAnnouncementChannelId = config.DiscordStreamAnnouncementChannelId,
                DiscordUserLinkingChannelId = config.DiscordUserLinkingChannelId,
                DiscordUserCommandsChannelId = config.DiscordUserCommandsChannelId,
                DiscordUserRankUpAnnouncementChannelId = config.DiscordUserRankUpAnnouncementChannelId,
                DiscordGiveawayChannelId = config.DiscordGiveawayChannelId,
                DiscordReactionRoleChannelId = config.DiscordReactionRoleChannelId,
                DiscordGeneralChannelId = config.DiscordGeneralChannelId,
                DiscordBanRoleChannelId = config.DiscordBanRoleChannelId,
                DiscordModeratorRoleId = config.DiscordModeratorRoleId
                };
        }
    }
}
