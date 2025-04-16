using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Linking
{
    //TODO: test method
    public class DiscordLinkingData(AppDbContext context, IDiscordService discordService) : IDiscordLinkingData
    {
        public async Task<string> NewLinkRequest(DiscordCommand command)
        {
            if (command.CommandText == null)
            {
                return "Please provide a Twitch username.";
            }

            var twitchUsername = command.CommandText.Trim().ToLower();

            var user = await context.ChannelUsers
                .FirstOrDefaultAsync(x => x.TwitchUsername == twitchUsername);

            if (user == null)
            {
                return $"No user found with the Twitch username supplied";
            }

            if (user.DiscordUserId != 0)
            {
                return $"This Twitch user is already linked to a Discord account.";
            }

            var existingRequest = await context.DiscordLinkRequests
                .FirstOrDefaultAsync(x => x.TwitchUsername == twitchUsername);

            var channel = await context.Channels
                .FirstAsync(x => x.DiscordGuildId == command.GuildId);

            if (existingRequest != null)
            {
                return $"A link request already exists for this Twitch username. Please type ```!link {existingRequest.TwitchLinkCode}``` in https://twitch.tv/{channel.BroadcasterTwitchChannelName} to link your account";
            }

            // create the link request
            var linkRequest = new DiscordLinkRequests
            {
                TwitchUsername = twitchUsername,
                DiscordUserId = command.UserId,
                TwitchLinkCode = new Random().Next(0, 999999)
            };

            await context.DiscordLinkRequests.AddAsync(linkRequest);
            await context.SaveChangesAsync();

            return $"A link request has been created for the Twitch username {twitchUsername}. Please type ```!link {linkRequest.TwitchLinkCode}``` in https://twitch.tv/{channel.BroadcasterTwitchChannelName} to link your account";
        }

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
                var guild = discordService.Client.GetGuild(broadcaster.DiscordGuildId!.Value);
                var user = guild.GetUser(dbUser.DiscordUserId);

                if (user == null)
                {
                    continue;
                }

                foreach(var watchtime in watchtimeRanksEarnedByUser)
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
            var guild = discordService.Client.GetGuild(guildId);
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
            var guild = discordService.Client.GetGuild(channel.DiscordGuildId ?? 0);

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
