namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordWordleData
    {
        /// <summary>
        /// Makes a guess at today's word
        /// </summary>
        /// <returns>The private reply showing the board, plus a spoiler free message for the channel once the game has finished</returns>
        Task<(string Response, string? PublicMessage)> Guess(ulong guildId, ulong userId, string guess);

        /// <summary>
        /// Shows the user's board for today without guessing
        /// </summary>
        Task<string> GetBoard(ulong guildId, ulong userId);
    }
}
