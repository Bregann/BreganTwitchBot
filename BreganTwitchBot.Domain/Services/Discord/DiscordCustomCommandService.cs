using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord
{
    /// <summary>
    /// Answers a channel's custom commands in Discord.
    ///
    /// These are the same commands as in Twitch chat, read from the same per channel table, so
    /// they only have to be set up once.
    /// </summary>
    public class DiscordCustomCommandService(AppDbContext context) : IDiscordCustomCommandService
    {
        public async Task<string?> TryHandleCustomCommand(ulong guildId, ulong channelId, string username, string messageContent, bool isMod)
        {
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return null;
            }

            var commandName = messageContent.Split(' ')[0].ToLower();

            if (!commandName.StartsWith('!'))
            {
                return null;
            }

            var channel = await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);

            if (channel == null)
            {
                return null;
            }

            // commands are kept to the commands channel so they can't be used to spam
            // elsewhere, though mods can use them anywhere
            var commandsChannelId = channel.ChannelConfig.DiscordUserCommandsChannelId;

            if (commandsChannelId != null && channelId != commandsChannelId && !isMod)
            {
                return null;
            }

            var command = await context.CustomCommands
                .FirstOrDefaultAsync(x => x.ChannelId == channel.Id && x.CommandName == commandName);

            if (command == null)
            {
                return null;
            }

            command.TimesUsed++;
            command.LastUsed = DateTime.UtcNow;
            await context.SaveChangesAsync();

            Log.Information($"[Discord Custom Commands] {commandName} used in {guildId}");

            // the old bot skipped commands using these placeholders, as it had nothing to put
            // in them. The values are available here, so they are filled in like the twitch side
            return command.CommandText
                .Replace("[count]", command.TimesUsed.ToString())
                .Replace("[user]", $"@{username}");
        }
    }
}
