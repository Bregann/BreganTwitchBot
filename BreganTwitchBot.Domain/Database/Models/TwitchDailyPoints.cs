using BreganTwitchBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
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
        public required PointsClaimType PointsClaimType { get; set; }

        [Required]
        public required int CurrentStreak { get; set; }

        [Required]
        public required int HighestStreak { get; set; }

        [Required]
        public required int TotalTimesClaimed { get; set; }

        [Required]
        public required long TotalPointsClaimed { get; set; }

        [Required]
        public required DateTime PointsLastClaimed { get; set; }

        [Required]
        public required bool PointsClaimed { get; set; }
    }
}
