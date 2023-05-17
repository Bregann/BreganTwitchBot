namespace BreganTwitchBot.Web.Data.UserSearch
{
    public class UserSearchData
    {
        public string UserId { get; set; }
        public string TwitchUsername { get; set; }
        public long MessagesSent { get; set; }
        public long MinutesInStream { get; set; }
        public DateTime LastSeenDate { get; set; }
        public long PointsGambled { get; set; }
        public long PointsWon { get; set; }
        public long PointsLost { get; set; }
        public long TotalSpins { get; set; }
        public long Tier1Wins { get; set; }
        public long Tier2Wins { get; set; }
        public long Tier3Wins { get; set; }
        public long JackpotWins { get; set; }
        public long SmorcWins { get; set; }
        public long CurrentStreak { get; set; }
        public long HighestStreak { get; set; }
        public long TotalTimesClaimed { get; set; }
        public long TotalPointsClaimed { get; set; }
        public long MarblesWins { get; set; }
        public long DiscordDailyTotalClaims { get; set; }
        public long DiscordLevel { get; set; }
        public long DiscordXP { get; set; }
        public long MinutesWatchedThisStream { get; set; }
        public long MinutesWatchedThisWeek { get; set; }
        public long MinutesWatchedThisMonth { get; set; }
        public long TwitchBossesDone { get; set; }
        public long TwitchBossesPointsWon { get; set; }
    }
}