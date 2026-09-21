using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// How giveaway entries are weighted in a channel. The old bot hardcoded these, so they are
    /// per channel rows here for the website to edit later.
    /// </summary>
    public class DiscordGiveawayConfig
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        /// <summary>
        /// Minutes of watchtime that earn one entry. The old bot used 3600 (60 hours).
        /// </summary>
        [Required]
        public required int MinutesPerEntry { get; set; }

        /// <summary>
        /// Discord xp that earns one entry. The old bot used 1000.
        /// </summary>
        [Required]
        public required int XpPerEntry { get; set; }

        /// <summary>
        /// The most entries discord xp alone can earn. The old bot capped this at 30.
        /// </summary>
        [Required]
        public required int MaxXpEntries { get; set; }

        /// <summary>
        /// Whether each stream rank a user has earned grants an extra entry
        /// </summary>
        [Required]
        public required bool RanksGrantEntries { get; set; }
    }
}
