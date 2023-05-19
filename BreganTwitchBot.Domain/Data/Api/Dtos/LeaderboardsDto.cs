namespace BreganTwitchBot.Domain.Data.Api.Dtos
{
    public class GetLeaderboardDto
    {
        public required string LeaderboardName { get; set; }
        public required List<LeaderboardDto> Leaderboards { get; set; }
    }
    public class LeaderboardDto
    {
        public required long Position { get; set; }
        public required string Username { get; set; }
        public required string Amount { get; set; }
    }
}
