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

    public class GetChannelSummaryResponse
    {
        public required string BroadcasterChannelName { get; set; }
        public required bool IsLive { get; set; }
        public required string PointsName { get; set; }
        public required bool SubathonActive { get; set; }
        public required int TrackedViewers { get; set; }
        public required long TotalStreams { get; set; }
    }

    public class GetStreamHistoryResponse
    {
        public required long StreamId { get; set; }
        public required DateTime StreamStarted { get; set; }
        public DateTime? StreamEnded { get; set; }
        public required double AvgViewCount { get; set; }
        public required long PeakViewerCount { get; set; }
        public required long MessagesReceived { get; set; }
        public required long NewFollowers { get; set; }
        public required long NewSubscribers { get; set; }
        public required long BitsDonated { get; set; }
        public required long UniquePeople { get; set; }
        public required string Uptime { get; set; }
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
