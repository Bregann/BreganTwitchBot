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
        public required bool WeeklyClaimed {  get; set; }
        public required int CurrentWeeklyStreak { get; set; }
        public required int HighestWeeklyStreak { get; set; }
        public required bool TotalWeeklyClaimed { get; set; }
        public required bool MonthlyClaimed { get; set; }
        public required int CurrentMonthlyStreak { get; set; }
        public required int HighestMonthlyStreak { get; set; }
        public required bool TotalMonthlyClaimed { get; set; }
        public required bool YearlyClaimed { get; set; }
        public required int CurrentYearlyStreak { get; set; }
        public required int HighestYearlyStreak { get; set; }
        public required bool TotalYearlyClaimed { get; set; }
    }
}