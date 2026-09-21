using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// A giveaway started in a Discord channel. Requirements are captured per giveaway so each
    /// one can set its own entry bar rather than sharing a single hardcoded rule.
    /// </summary>
    public class DiscordGiveaway
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        /// <summary>
        /// The discord interaction id the entry buttons carry
        /// </summary>
        [Required]
        public required string GiveawayId { get; set; }

        [Required]
        public required ulong StartedByDiscordUserId { get; set; }

        [Required]
        public required DateTime StartedAt { get; set; }

        [Required]
        public required bool Active { get; set; }

        /// <summary>
        /// Minutes of watchtime needed before a user can enter. 0 means anyone can enter.
        /// </summary>
        [Required]
        public required int MinimumWatchtimeMinutes { get; set; }

        /// <summary>
        /// The rank a user must have reached to enter, if any
        /// </summary>
        [ForeignKey(nameof(RequiredRank))]
        public int? RequiredRankId { get; set; }
        public virtual ChannelRank? RequiredRank { get; set; }

        public ulong? WinnerDiscordUserId { get; set; }

        public virtual ICollection<DiscordGiveawayEntry> Entries { get; set; } = null!;
    }
}
