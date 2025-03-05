using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class TwitchDailyPoints
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public int ChannelUserId { get; set; }
        public virtual ChannelUser User { get; set; } = null!;

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required int CurrentDailyStreak { get; set; }

        [Required]
        public required int HighestDailyStreak { get; set; }

        [Required]
        public required int TotalTimesDailyClaimed { get; set; }

        [Required]
        public required long TotalPointsClaimed { get; set; }

        [Required]
        public required DateTime PointsLastClaimed { get; set; }

        [Required]
        public required bool PointsClaimedThisStream { get; set; }

        [Required]
        public required bool WeeklyPointsClaimed { get; set; }

        [Required]
        public required int CurrentlyWeeklyStreak { get; set; }

        [Required]
        public required int HighestWeeklyStreak { get; set; }

        [Required]
        public required int TotalTimesWeeklyClaimed { get; set; }

        [Required]
        public required bool MonthlyPointsClaimed { get; set; }

        [Required]
        public required int CurrentMonthlyStreak { get; set; }

        [Required]
        public required int HighestMonthlyStreak { get; set; }

        [Required]
        public required int TotalTimesMonthlyClaimed { get; set; }

        [Required]
        public required bool YearlyPointsClaimed { get; set; }

        [Required]
        public required int CurrentYearlyStreak { get; set; }

        [Required]
        public required int HighestYearlyStreak { get; set; }

        [Required]
        public required int TotalTimesYearlyClaimed { get; set; }
    }
}
