using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordWordleData
    {
        /// <summary>
        /// Makes a guess at today's word
        /// </summary>
        Task<WordleResponse> Guess(ulong guildId, ulong userId, string guess);

        /// <summary>
        /// Shows the user's board for today without guessing
        /// </summary>
        Task<WordleResponse> GetBoard(ulong guildId, ulong userId);
    }
}
