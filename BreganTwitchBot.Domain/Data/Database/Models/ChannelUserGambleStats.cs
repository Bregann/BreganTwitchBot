using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelUserGambleStats
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public int ChannelUserId { get; set; }
        public virtual ChannelUser ChannelUser { get; set; } = null!;

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required long PointsGambled { get; set; }

        [Required]
        public required long PointsWon { get; set; }

        [Required]
        public required long PointsLost { get; set; }

        [Required]
        public required int TotalSpins { get; set; }

        [Required]
        public required int Tier1Wins { get; set; }

        [Required]
        public required int Tier2Wins { get; set; }

        [Required]
        public required int Tier3Wins { get; set; }

        [Required]
        public required int JackpotWins { get; set; }

        [Required]
        public required int SmorcWins { get; set; }

        [Required]
        public required int BookWins { get; set; }
    }
}
