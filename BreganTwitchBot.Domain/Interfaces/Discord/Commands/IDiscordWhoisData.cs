using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordWhoisData
    {
        /// <summary>
        /// Looks a user up by either their Discord account or their Twitch username and returns
        /// what the bot knows about them
        /// </summary>
        Task<DiscordEmbedData> HandleWhoisCommandAsync(WhoisCommand command);
    }
}
