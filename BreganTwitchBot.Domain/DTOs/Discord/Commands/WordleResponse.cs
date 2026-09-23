namespace BreganTwitchBot.Domain.DTOs.Discord.Commands
{
    /// <param name="Response">The private reply showing the board</param>
    /// <param name="CanGuess">Whether the game is still going, so the guess button should be offered</param>
    /// <param name="PublicMessage">A spoiler free message for the channel once the game has finished</param>
    public record WordleResponse(string Response, bool CanGuess, string? PublicMessage = null);
}
