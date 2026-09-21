namespace BreganTwitchBot.Domain.DTOs.Api
{
    public class GetLeaderboardResponse
    {
        public required string LeaderboardName { get; set; }
        public required string BroadcasterChannelName { get; set; }
        public required List<LeaderboardPositionResponse> Positions { get; set; }
    }

    public class LeaderboardPositionResponse
    {
        public required int Position { get; set; }
        public required string Username { get; set; }
        public required long Value { get; set; }
    }

    public class GetCustomCommandResponse
    {
        public required string CommandName { get; set; }
        public required string CommandText { get; set; }
        public required int TimesUsed { get; set; }
    }

    public class GetSubathonStatusResponse
    {
        public required bool Active { get; set; }
        public required int SecondsLeft { get; set; }
        public required string TotalTimeAdded { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndsAt { get; set; }
    }

    public class GetSubathonLeaderboardResponse
    {
        public required List<SubathonContributorResponse> TopBitsDonators { get; set; }
        public required List<SubathonContributorResponse> TopSubGifters { get; set; }
    }

    public class SubathonContributorResponse
    {
        public required int Position { get; set; }
        public required string Username { get; set; }
        public required long Amount { get; set; }
    }
}
