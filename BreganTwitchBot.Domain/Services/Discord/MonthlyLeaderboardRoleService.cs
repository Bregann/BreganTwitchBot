using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord
{
    /// <summary>
    /// Hands out the monthly bits and gifted subs leaderboard roles.
    ///
    /// The old bot resolved roles by hardcoded name and, when clearing them, only ever removed
    /// the role from the first holder it found via an enumerator, so a role left on a second user
    /// stayed there forever. This removes from every holder and is driven by configured role ids.
    /// </summary>
    public class MonthlyLeaderboardRoleService(AppDbContext context, IDiscordClientProvider discordClientProvider) : IMonthlyLeaderboardRoleService
    {
        public async Task UpdateMonthlyLeaderboardRolesAsync()
        {
            var channels = await context.Channels
                .Where(x => x.ChannelConfig.DiscordGuildId != null)
                .ToListAsync();

            foreach (var channel in channels)
            {
                try
                {
                    await UpdateChannelAsync(channel.Id, channel.ChannelConfig.DiscordGuildId!.Value);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Monthly Leaderboards] Error updating roles for {channel.BroadcasterTwitchChannelName}");
                }
            }
        }

        private async Task UpdateChannelAsync(int channelId, ulong guildId)
        {
            var configuredRoles = await context.MonthlyLeaderboardRoles
                .Where(x => x.ChannelId == channelId)
                .ToListAsync();

            if (configuredRoles.Count == 0)
            {
                return;
            }

            var guild = discordClientProvider.Client.GetGuild(guildId);

            if (guild == null)
            {
                Log.Warning($"[Monthly Leaderboards] Guild {guildId} not found");
                return;
            }

            await guild.DownloadUsersAsync();

            // the first of the month wipes the totals and takes every role back off
            if (DateTime.UtcNow.Day == 1)
            {
                await ClearRolesAndResetTotals(channelId, guild, configuredRoles.Select(x => x.DiscordRoleId).Distinct());
                return;
            }

            foreach (var leaderboardType in Enum.GetValues<MonthlyLeaderboardType>())
            {
                var rolesForType = configuredRoles
                    .Where(x => x.LeaderboardType == leaderboardType)
                    .OrderBy(x => x.Position)
                    .ToList();

                if (rolesForType.Count == 0)
                {
                    continue;
                }

                var leaders = await GetLeaders(channelId, leaderboardType, rolesForType.Count);

                for (var i = 0; i < rolesForType.Count; i++)
                {
                    var role = guild.GetRole(rolesForType[i].DiscordRoleId);

                    if (role == null)
                    {
                        Log.Warning($"[Monthly Leaderboards] Role {rolesForType[i].DiscordRoleId} not found in guild {guildId}");
                        continue;
                    }

                    // nobody at this position this month, so make sure the role is unheld
                    var discordUserId = i < leaders.Count ? leaders[i] : (ulong?)null;

                    foreach (var currentHolder in guild.Users.Where(x => x.Roles.Any(r => r.Id == role.Id)).ToList())
                    {
                        if (currentHolder.Id != discordUserId)
                        {
                            await currentHolder.RemoveRoleAsync(role);
                        }
                    }

                    if (discordUserId == null)
                    {
                        continue;
                    }

                    var newHolder = guild.GetUser(discordUserId.Value);

                    if (newHolder != null && !newHolder.Roles.Any(r => r.Id == role.Id))
                    {
                        await newHolder.AddRoleAsync(role);
                        Log.Information($"[Monthly Leaderboards] Gave {role.Name} to {newHolder.Username}");
                    }
                }
            }
        }

        /// <summary>
        /// The discord ids of the top contributors this month, best first
        /// </summary>
        private async Task<List<ulong>> GetLeaders(int channelId, MonthlyLeaderboardType leaderboardType, int take)
        {
            var query = context.ChannelUserStats
                .Where(x => x.ChannelId == channelId && x.User.DiscordUserId != 0);

            query = leaderboardType switch
            {
                MonthlyLeaderboardType.BitsDonated => query.Where(x => x.BitsDonatedThisMonth > 0).OrderByDescending(x => x.BitsDonatedThisMonth),
                MonthlyLeaderboardType.SubsGifted => query.Where(x => x.GiftedSubsThisMonth > 0).OrderByDescending(x => x.GiftedSubsThisMonth),
                _ => query
            };

            return await query
                .Take(take)
                .Select(x => x.User.DiscordUserId)
                .ToListAsync();
        }

        private async Task ClearRolesAndResetTotals(int channelId, SocketGuild guild, IEnumerable<ulong> roleIds)
        {
            foreach (var roleId in roleIds)
            {
                var role = guild.GetRole(roleId);

                if (role == null)
                {
                    continue;
                }

                // the old bot only removed the role from the first holder it found
                foreach (var holder in guild.Users.Where(x => x.Roles.Any(r => r.Id == role.Id)).ToList())
                {
                    await holder.RemoveRoleAsync(role);
                }
            }

            var statsToReset = await context.ChannelUserStats
                .Where(x => x.ChannelId == channelId && (x.BitsDonatedThisMonth != 0 || x.GiftedSubsThisMonth != 0))
                .ToListAsync();

            foreach (var stats in statsToReset)
            {
                stats.BitsDonatedThisMonth = 0;
                stats.GiftedSubsThisMonth = 0;
            }

            await context.SaveChangesAsync();

            Log.Information($"[Monthly Leaderboards] Cleared roles and reset {statsToReset.Count} monthly totals");
        }
    }
}
