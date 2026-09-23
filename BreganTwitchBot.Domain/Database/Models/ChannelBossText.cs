using BreganTwitchBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// A boss name suffix or elimination reason a channel can use.
    ///
    /// These were two hardcoded lists, so every channel shared the same jokes and adding one
    /// meant a deploy. A channel with no rows falls back to the original defaults, so the
    /// feature still works before anything is configured.
    /// </summary>
    public class ChannelBossText
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required BossTextType TextType { get; set; }

        [Required]
        public required string Text { get; set; }
    }
}
