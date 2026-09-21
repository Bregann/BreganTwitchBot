using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.GeneralCommands
{
    /// <summary>
    /// The Discord side of the general commands.
    ///
    /// The counts come from the channel linked to the guild rather than a single hardcoded
    /// broadcaster, and the mute role is looked up by id from the channel's config instead of
    /// by the name "mute", which the old bot did and which broke if the role was renamed.
    /// </summary>
    public class DiscordGeneralCommandsData(
        AppDbContext context,
        IDiscordClientProvider discordClientProvider,
        ITwitchApiConnection twitchApiConnection,
        ITwitchApiInteractionService twitchApiInteractionService) : IDiscordGeneralCommandsData
    {
        public async Task<string> GetFollowerCount(ulong guildId)
        {
            var channel = await GetChannelForGuild(guildId);

            if (channel == null)
            {
                return "This server isn't linked to a channel";
            }

            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(channel.BroadcasterTwitchChannelName);

            if (apiClient == null)
            {
                return "lol it broke, try again";
            }

            try
            {
                var followerCount = await twitchApiInteractionService.GetChannelFollowerCountAsync(apiClient.ApiClient, channel.BroadcasterTwitchChannelId, apiClient.TwitchChannelClientId);
                return $"{channel.BroadcasterTwitchChannelName} has {followerCount:N0} followers";
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Discord General] Error getting the follower count for {channel.BroadcasterTwitchChannelName}");
                return "lol it broke, try again";
            }
        }

        public async Task<string> GetSubscriberCount(ulong guildId)
        {
            var channel = await GetChannelForGuild(guildId);

            if (channel == null)
            {
                return "This server isn't linked to a channel";
            }

            // the sub count is only readable with the broadcaster's own token
            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(channel.BroadcasterTwitchChannelName);

            if (apiClient == null)
            {
                return "There has been an error with your request. Please try again";
            }

            try
            {
                var subCount = await twitchApiInteractionService.GetChannelSubscriberCountAsync(apiClient.ApiClient, channel.BroadcasterTwitchChannelId);
                return $"{channel.BroadcasterTwitchChannelName} has {subCount:N0} subs!";
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Discord General] Error getting the sub count for {channel.BroadcasterTwitchChannelName}");
                return "There has been an error with your request. Please try again";
            }
        }

        public async Task<string> SetUserMuted(ulong guildId, ulong userId, bool muted)
        {
            var channel = await GetChannelForGuild(guildId);

            if (channel == null)
            {
                return "This server isn't linked to a channel";
            }

            var muteRoleId = channel.ChannelConfig.DiscordMuteRoleId;

            if (muteRoleId == null)
            {
                return "No mute role is configured for this server";
            }

            var guild = discordClientProvider.Client.GetGuild(guildId);

            if (guild == null)
            {
                return "I couldn't find that server";
            }

            var role = guild.GetRole(muteRoleId.Value);

            if (role == null)
            {
                Log.Warning($"[Discord General] Mute role {muteRoleId} is configured but missing from guild {guildId}");
                return "The configured mute role no longer exists";
            }

            var user = guild.GetUser(userId);

            if (user == null)
            {
                return "I couldn't find that user in the server";
            }

            if (muted)
            {
                await user.AddRoleAsync(role);
                return "That melvin has been muted bruv";
            }

            await user.RemoveRoleAsync(role);
            return "That melvin has been unmuted bruv";
        }

        private async Task<Database.Models.Channel?> GetChannelForGuild(ulong guildId)
        {
            return await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);
        }
    }
}
