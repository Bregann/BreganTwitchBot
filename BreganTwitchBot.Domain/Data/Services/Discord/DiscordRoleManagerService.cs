using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordRoleManagerService(AppDbContext context, IDiscordClientProvider discordClientProvider) : IDiscordRoleManagerService
    {

        public async Task AddRolesToUserOnLink(string twitchUserId)
        {
            var broadcastersWithDiscordEnabled = await context.Channels
                .Where(x => x.DiscordGuildId != null)
                .ToListAsync();

            var dbUser = await context.ChannelUsers
                .FirstAsync(x => x.TwitchUserId == twitchUserId);

            // loop through each guild and check if the user has earned any watchtime roles
            foreach (var broadcaster in broadcastersWithDiscordEnabled)
            {
                var watchtimeRanksEarnedByUser = await context.ChannelUserRankProgress.Where(x => x.ChannelId == broadcaster.Id && x.ChannelUser.TwitchUserId == twitchUserId).ToListAsync();

                if (watchtimeRanksEarnedByUser.Count == 0)
                {
                    continue;
                }

                // check if the user is actually in the guild
                var guild = discordClientProvider.Client.GetGuild(broadcaster.DiscordGuildId!.Value);
                var user = guild.GetUser(dbUser.DiscordUserId);

                if (user == null)
                {
                    continue;
                }

                foreach (var watchtime in watchtimeRanksEarnedByUser)
                {
                    if (watchtime.ChannelRank.DiscordRoleId != null)
                    {
                        var role = guild.GetRole(watchtime.ChannelRank.DiscordRoleId.Value);

                        if (role != null)
                        {
                            await user.AddRoleAsync(role);
                            Log.Information($"Added role {role.Name} to user {user.Username} in guild {guild.Name}");
                        }
                    }
                }
            }
        }

        public async Task AddRolesToUserOnGuildJoin(ulong discordUserId, ulong guildId)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.DiscordGuildId == guildId);

            if (channel == null)
            {
                Log.Information($"No channel found for guild {guildId}");
                return;
            }

            var dbUser = await context.ChannelUsers.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);

            if (dbUser == null)
            {
                Log.Information($"No user found with Discord ID {discordUserId}");
                return;
            }

            // check if the user has earned any watchtime roles
            var watchtimeRanksEarnedByUser = await context.ChannelUserRankProgress.Where(x => x.ChannelId == channel.Id && x.ChannelUser.TwitchUserId == dbUser.TwitchUserId).ToListAsync();

            if (watchtimeRanksEarnedByUser.Count == 0)
            {
                Log.Information($"No watchtime ranks found for user {dbUser.TwitchUserId} in channel {channel.BroadcasterTwitchChannelName}");
                return;
            }

            // check if the user is actually in the guild
            var guild = discordClientProvider.Client.GetGuild(guildId);
            var user = guild.GetUser(discordUserId);

            foreach (var watchtime in watchtimeRanksEarnedByUser)
            {
                if (watchtime.ChannelRank.DiscordRoleId != null)
                {
                    var role = guild.GetRole(watchtime.ChannelRank.DiscordRoleId.Value);

                    if (role != null)
                    {
                        await user.AddRoleAsync(role);
                        Log.Information($"Added role {role.Name} to user {user.Username} in guild {guild.Name}");
                    }
                }
            }
        }

        public async Task ApplyRoleOnDiscordWatchtimeRankup(string twitchUserId, string broadcasterChannelId)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelId == broadcasterChannelId);

            if (channel == null)
            {
                Log.Information($"No channel found for stream {broadcasterChannelId}");
                return;
            }

            var dbUser = await context.ChannelUsers.FirstAsync(x => x.TwitchUserId == twitchUserId);

            // check if the user is actually in the guild
            var guild = discordClientProvider.Client.GetGuild(channel.DiscordGuildId ?? 0);

            if (guild == null)
            {
                Log.Information($"No guild found with ID {channel.DiscordGuildId}");
                return;
            }

            var user = guild.GetUser(dbUser.DiscordUserId);

            if (user == null)
            {
                Log.Information($"No user found with Discord ID {dbUser.DiscordUserId}");
                return;
            }

            // check if the user has earned any watchtime roles
            var watchtimeRanksEarnedByUser = await context.ChannelUserRankProgress.Where(x => x.ChannelId == channel.Id && x.ChannelUser.TwitchUserId == dbUser.TwitchUserId).ToListAsync();

            foreach (var watchtime in watchtimeRanksEarnedByUser)
            {
                if (watchtime.ChannelRank.DiscordRoleId != null)
                {
                    var role = guild.GetRole(watchtime.ChannelRank.DiscordRoleId.Value);
                    if (role != null)
                    {
                        await user.AddRoleAsync(role);
                        Log.Information($"Added role {role.Name} to user {user.Username} in guild {guild.Name}");
                    }
                }
            }
        }
    }
}
