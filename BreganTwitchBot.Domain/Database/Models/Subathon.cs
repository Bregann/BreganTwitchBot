using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// What a user has contributed to a channel's subathon. One row per user per channel.
    /// </summary>
    public class Subathon
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }

        public virtual Channel Channel { get; set; } = null!;

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public required int ChannelUserId { get; set; }

        public virtual ChannelUser ChannelUser { get; set; } = null!;

        [Required]
        public required long BitsDonated { get; set; }

        [Required]
        public required int SubsGifted { get; set; }

        /// <summary>
        /// Total time this user has added to the subathon
        /// </summary>
        [Required]
        public required TimeSpan TimeAdded { get; set; }
    }
}
