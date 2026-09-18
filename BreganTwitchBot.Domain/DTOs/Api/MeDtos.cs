namespace BreganTwitchBot.Domain.DTOs.Api
{
    public class GetMyStatsResponse
    {
        public required string TwitchUsername { get; set; }
        public required List<MyChannelStatsResponse> Channels { get; set; }
    }

    public class MyChannelStatsResponse
    {
        public required string BroadcasterChannelName { get; set; }
        public required string PointsName { get; set; }
        public required long Points { get; set; }

        public required int MinutesInStream { get; set; }
        public required int MinutesWatchedThisWeek { get; set; }
        public required int MinutesWatchedThisMonth { get; set; }

        public required int TotalMessages { get; set; }
        public required int MarblesWins { get; set; }

        /// <summary>
        /// The highest rank they have reached, if any
        /// </summary>
        public string? CurrentRank { get; set; }

        /// <summary>
        /// The next rank up, and how far off it they are
        /// </summary>
        public string? NextRank { get; set; }
        public int? MinutesUntilNextRank { get; set; }

        public required int CurrentDailyStreak { get; set; }
        public required int HighestDailyStreak { get; set; }

        public required long PointsWon { get; set; }
        public required long PointsLost { get; set; }

        public int? DiscordLevel { get; set; }
        public long? DiscordXp { get; set; }
    }
}
