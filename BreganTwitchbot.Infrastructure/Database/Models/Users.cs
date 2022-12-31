using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Users
    {
        [Key]
        [Required]
        public string TwitchUserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public bool InStream { get; set; }

        [Required]
        public bool IsSub { get; set; }

        [Required]
        public int MinutesInStream { get; set; }

        [Required]
        public long Points { get; set; }

        [Required]
        public bool IsSuperMod { get; set; }

        [Required]
        public int TotalMessages { get; set; }

        [Required]
        public ulong DiscordUserId { get; set; }

        [Required]
        public DateTime LastSeenDate { get; set; }

        [Required]
        public long PointsGambled { get; set; }

        [Required]
        public long PointsWon { get; set; }

        [Required]
        public long PointsLost { get; set; }

        [Required]
        public int TotalSpins { get; set; }

        [Required]
        public int Tier1Wins { get; set; }

        [Required]
        public int Tier2Wins { get; set; }

        [Required]
        public int Tier3Wins { get; set; }

        [Required]
        public int JackpotWins { get; set; }

        [Required]
        public int SmorcWins { get; set; }

        [Required]
        public int CurrentStreak { get; set; }

        [Required]
        public int HighestStreak { get; set; }

        [Required]
        public int TotalTimesClaimed { get; set; }

        [Required]
        public long TotalPointsClaimed { get; set; }

        [Required]
        public DateTime PointsLastClaimed { get; set; }

        [Required]
        public bool PointsClaimedThisStream { get; set; }

        [Required]
        public int GiftedSubsThisMonth { get; set; }

        [Required]
        public int BitsDonatedThisMonth { get; set; }

        [Required]
        public int MarblesWins { get; set; }

        [Required]
        public int DiceRolls { get; set; }

        [Required]
        public int BonusDiceRolls { get; set; }

        [Required]
        public int DiscordDailyStreak { get; set; }

        [Required]
        public int DiscordDailyTotalClaims { get; set; }

        [Required]
        public bool DiscordDailyClaimed { get; set; }

        [Required]
        public int DiscordLevel { get; set; }

        [Required]
        public int DiscordXp { get; set; }

        [Required]
        public bool DiscordLevelUpNotifsEnabled { get; set; }

        [Required]
        public int PrestigeLevel { get; set; }

        [Required]
        public int MinutesWatchedThisStream { get; set; }

        [Required]
        public int MinutesWatchedThisWeek { get; set; }

        [Required]
        public int MinutesWatchedThisMonth { get; set; }

        [Required]
        public int BossesDone { get; set; }

        [Required]
        public long BossesPointsWon { get; set; }

        [Required]
        public int TimeoutStrikes { get; set; }

        [Required]
        public int WarnStrikes { get; set; }

        [Required]
        public bool Rank1Applied { get; set; }

        [Required]
        public bool Rank2Applied { get; set; }

        [Required]
        public bool Rank3Applied { get; set; }

        [Required]
        public bool Rank4Applied { get; set; }

        [Required]
        public bool Rank5Applied { get; set; }
    }
}