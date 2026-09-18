using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// A role members can give themselves from a button panel.
    ///
    /// The old bot hardcoded twelve of these as a switch mapping button ids to role names, so
    /// adding one meant a deploy and renaming a role in Discord broke it. They are per channel
    /// rows holding role ids here.
    /// </summary>
    public class DiscordSelfAssignRole
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required ulong DiscordRoleId { get; set; }

        /// <summary>
        /// The label shown on the button and used in the reply
        /// </summary>
        [Required]
        public required string DisplayName { get; set; }

        /// <summary>
        /// Optional emoji for the button
        /// </summary>
        public string? Emoji { get; set; }

        /// <summary>
        /// Controls the order the buttons appear in
        /// </summary>
        [Required]
        public required int SortOrder { get; set; }
    }
}
