using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordLevellingData
    {
        Task<string> HandleToggleLevelUpCommand(DiscordCommand command);

        /// <summary>
        /// Somebody's Discord level, xp and progress towards the next level
        /// </summary>
        Task<DiscordEmbedData> HandleLevelCommand(ulong guildId, ulong discordUserId, string displayName);
    }
}
