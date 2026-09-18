using BreganTwitchBot.Domain.Database.Models;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordSelfAssignRoleData
    {
        /// <summary>
        /// The self assignable roles configured for the channel linked to this guild
        /// </summary>
        Task<List<DiscordSelfAssignRole>> GetRolesAsync(ulong guildId);

        /// <summary>
        /// Adds the role to the user if they don't have it, removes it if they do
        /// </summary>
        Task<(string Response, bool Ephemeral)> ToggleRoleAsync(ulong guildId, ulong userId, int roleConfigId);
    }
}
