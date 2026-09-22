using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// A user's entry into a giveaway. The old bot inserted one row per entry, so a user with 30
    /// entries meant 30 rows. The count is a column here instead.
    /// </summary>
    public class DiscordGiveawayEntry
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(DiscordGiveaway))]
        [Required]
        public int DiscordGiveawayId { get; set; }
        public virtual DiscordGiveaway DiscordGiveaway { get; set; } = null!;

        [Required]
        public required ulong DiscordUserId { get; set; }

        /// <summary>
        /// How many times this user is in the draw
        /// </summary>
        [Required]
        public required int Entries { get; set; }

        [Required]
        public required DateTime EnteredAt { get; set; }
    }
}
