namespace BreganTwitchBot.Domain.DTOs.Discord.Commands
{
    /// <summary>
    /// One day's game, as much of it as the stats need
    /// </summary>
    public record WordleGameSummary(DateOnly Date, int GuessCount, bool Solved);

    /// <param name="GuessDistribution">How many games were won in 1 guess, 2 guesses... up to 6</param>
    public record WordleStats(int Played, int Won, int CurrentStreak, int MaxStreak, int[] GuessDistribution)
    {
        public int WinPercentage => Played == 0 ? 0 : (int)Math.Round(Won * 100.0 / Played);
    }
}
