using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.SelfAssignRoles
{
    public class DiscordSelfAssignRoleData(AppDbContext context, IDiscordClientProvider discordClientProvider) : IDiscordSelfAssignRoleData
    {
        public async Task<List<DiscordSelfAssignRole>> GetRolesAsync(ulong guildId)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);

            if (channel == null)
            {
                return [];
            }

            return await context.DiscordSelfAssignRoles
                .Where(x => x.ChannelId == channel.Id)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        public async Task<(string Response, bool Ephemeral)> ToggleRoleAsync(ulong guildId, ulong userId, int roleConfigId)
        {
            var roleConfig = await context.DiscordSelfAssignRoles
                .FirstOrDefaultAsync(x => x.Id == roleConfigId && x.Channel.ChannelConfig.DiscordGuildId == guildId);

            if (roleConfig == null)
            {
                return ("That role isn't available any more", true);
            }

            var guild = discordClientProvider.Client.GetGuild(guildId);
            var user = guild?.GetUser(userId);

            if (guild == null || user == null)
            {
                return ("I couldn't find you in the server, try again", true);
            }

            var role = guild.GetRole(roleConfig.DiscordRoleId);

            // the old bot looked roles up by name, so a rename silently broke the button
            if (role == null)
            {
                Log.Warning($"[Self Assign Roles] Role {roleConfig.DiscordRoleId} is configured but missing from guild {guildId}");
                return ("That role no longer exists, let a mod know", true);
            }

            if (user.Roles.Any(x => x.Id == role.Id))
            {
                await user.RemoveRoleAsync(role);
                return ($"Removed the **{roleConfig.DisplayName}** role", true);
            }

            await user.AddRoleAsync(role);
            return ($"Given you the **{roleConfig.DisplayName}** role", true);
        }
    }
}
