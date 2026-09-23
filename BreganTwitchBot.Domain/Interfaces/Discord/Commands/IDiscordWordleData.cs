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

        /// <summary>
        /// Games played, win rate, streaks and how many guesses each win took
        /// </summary>
        /// <returns>The stats, or null if the server isn't linked to a channel</returns>
        Task<WordleStats?> GetStats(ulong guildId, ulong userId);
    }
}
