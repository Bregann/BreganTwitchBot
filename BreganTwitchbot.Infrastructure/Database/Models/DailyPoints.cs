using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class DailyPoints
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Users User { get; set; }

        public string TwitchUserId { get; set; }

        public required int CurrentStreak { get; set; }

        public required int HighestStreak { get; set; }

        public required int TotalTimesClaimed { get; set; }

        public required long TotalPointsClaimed { get; set; }

        public required DateTime PointsLastClaimed { get; set; }

        public required bool PointsClaimedThisStream { get; set; }

        public required int DiscordDailyStreak { get; set; }

        public required int DiscordDailyTotalClaims { get; set; }

        public required bool DiscordDailyClaimed { get; set; }
    }
}