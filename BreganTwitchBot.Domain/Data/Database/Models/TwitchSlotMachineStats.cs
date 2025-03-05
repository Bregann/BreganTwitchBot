using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class TwitchSlotMachineStats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required int Tier1Wins { get; set; }

        [Required]
        public required int Tier2Wins { get; set; }

        [Required]
        public required int Tier3Wins { get; set; }

        [Required]
        public required int JackpotWins { get; set; }

        [Required]
        public required int TotalSpins { get; set; }

        [Required]
        public required int SmorcWins { get; set; }

        [Required]
        public required long JackpotAmount { get; set; }
    }
}
