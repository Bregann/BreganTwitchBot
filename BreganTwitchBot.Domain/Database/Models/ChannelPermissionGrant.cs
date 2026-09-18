using BreganTwitchBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// One permission granted to one user in one channel.
    ///
    /// Kept as a row per permission rather than a flags column so each grant can
    /// record who made it and when, which matters when working out how somebody
    /// ended up able to change something.
    /// </summary>
    public class ChannelPermissionGrant
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public int ChannelUserId { get; set; }
        public virtual ChannelUser ChannelUser { get; set; } = null!;

        [Required]
        public required ChannelPermission Permission { get; set; }

        [Required]
        public required DateTime GrantedAt { get; set; }

        /// <summary>
        /// The twitch id of whoever granted it, kept as an id rather than a relation
        /// so the record survives the granter being removed
        /// </summary>
        [Required]
        public required string GrantedByTwitchUserId { get; set; }
    }
}
