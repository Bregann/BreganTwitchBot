namespace BreganTwitchBot.Domain.DTOs.Discord
{
    public class LeaderboardEntry
    {
        public required int Position { get; set; }
        public required string Username { get; set; }
        public required long Value { get; set; }
    }
}
