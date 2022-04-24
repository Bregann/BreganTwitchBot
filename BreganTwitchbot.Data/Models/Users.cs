using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Data.Models
{
    public class Users
    {
        [Key]
        [Column("twitchUserId")]
        public string TwitchUserId { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("inStream")]
        public bool InStream { get; set; }

        [Column("isSub")]
        public bool IsSub { get; set; }

        [Column("minutesInStream")]
        public int MinutesInStream { get; set; }

        [Column("points")]
        public long Points { get; set; }

        [Column("isSuperMod")]
        public bool IsSuperMod { get; set; }

        [Column("totalMessages")]
        public int TotalMessages { get; set; }

        [Column("discordUserId")]
        public ulong DiscordUserId { get; set; }

        [Column("lastSeenDate")]
        public DateTime LastSeenDate { get; set; }

        [Column("pointsGambled")]
        public long PointsGambled { get; set; }

        [Column("pointsWon")]
        public long PointsWon { get; set; }

        [Column("pointsLost")]
        public long PointsLost { get; set; }

        [Column("totalSpins")]
        public int TotalSpins { get; set; }

        [Column("tier1Wins")]
        public int Tier1Wins { get; set; }

        [Column("tier2Wins")]
        public int Tier2Wins { get; set; }

        [Column("tier3Wins")]
        public int Tier3Wins { get; set; }

        [Column("jackpotWins")]
        public int JackpotWins { get; set; }

        [Column("smorcWins")]
        public int SmorcWins { get; set; }

        [Column("currentStreak")]
        public int CurrentStreak { get; set; }

        [Column("highestStreak")]
        public int HighestStreak { get; set; }

        [Column("totalTimesClaimed")]
        public int TotalTimesClaimed { get; set; }

        [Column("totalPointsClaimed")]
        public long TotalPointsClaimed { get; set; }

        [Column("pointsLastClaimed")]
        public DateTime PointsLastClaimed { get; set; }

        [Column("pointsClaimedThisStream")]
        public bool PointsClaimedThisStream { get; set; }

        [Column("giftedSubsThisMonth")]
        public int GiftedSubsThisMonth { get; set; }

        [Column("bitsDonatedThisMonth")]
        public int BitsDonatedThisMonth { get; set; }

        [Column("marblesWins")]
        public int MarblesWins { get; set; }

        [Column("diceRolls")]
        public int DiceRolls { get; set; }

        [Column("bonusDiceRolls")]
        public int BonusDiceRolls { get; set; }

        [Column("discordDailyStreak")]
        public int DiscordDailyStreak { get; set; }

        [Column("discordDailyTotalClaims")]
        public int DiscordDailyTotalClaims { get; set; }

        [Column("discordDailyClaimed")]
        public bool DiscordDailyClaimed { get; set; }

        [Column("discordLevel")]
        public int DiscordLevel { get; set; }

        [Column("discordXp")]
        public int DiscordXp { get; set; }

        [Column("discordLevelUpNotifsEnabled")]
        public bool DiscordLevelUpNotifsEnabled { get; set; }

        [Column("prestigeLevel")]
        public int PrestigeLevel { get; set; }

        [Column("minutesWatchedThisStream")]
        public int MinutesWatchedThisStream { get; set; }

        [Column("minutesWatchedThisWeek")]
        public int MinutesWatchedThisWeek { get; set; }

        [Column("minutesWatchedThisMonth")]
        public int MinutesWatchedThisMonth { get; set; }

        [Column("bossesDone")]
        public int BossesDone { get; set; }

        [Column("bossesPointsWon")]
        public long BossesPointsWon { get; set; }

        [Column("timeoutStrikes")]
        public int TimeoutStrikes { get; set; }

        [Column("warnStrikes")]
        public int WarnStrikes { get; set; }

        [Column("rank1Applied")]
        public bool Rank1Applied { get; set; }

        [Column("rank2Applied")]
        public bool Rank2Applied { get; set; }

        [Column("rank3Applied")]
        public bool Rank3Applied { get; set; }

        [Column("rank4Applied")]
        public bool Rank4Applied { get; set; }

        [Column("rank5Applied")]
        public bool Rank5Applied { get; set; }
    }
}
