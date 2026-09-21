using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Helpers
{
    /// <summary>
    /// Looking up the channel behind a Discord guild.
    ///
    /// A guild is linked to a channel through ChannelConfig.DiscordGuildId, so every Discord
    /// command that touches channel data starts with the same query. These wrap it up so the
    /// join is written once.
    /// </summary>
    public static class ChannelQueryHelper
    {
        /// <summary>
        /// The channel linked to the guild, or null when the server has not been linked to one.
        /// Use this when the command can tell the user the server is not set up
        /// </summary>
        public static async Task<Channel?> GetChannelForGuild(this AppDbContext context, ulong guildId)
        {
            return await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);
        }

        /// <summary>
        /// The channel linked to the guild, throwing when the server has not been linked to one.
        /// Use this where a missing channel means the bot is misconfigured rather than being
        /// something the user can act on
        /// </summary>
        public static async Task<Channel> GetRequiredChannelForGuild(this AppDbContext context, ulong guildId)
        {
            return await context.Channels.FirstAsync(x => x.ChannelConfig.DiscordGuildId == guildId);
        }
    }
}
