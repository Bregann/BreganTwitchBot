using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// A message the bot posts to chat on a timer, for the things a channel repeats all
    /// stream - the discord link, store codes and so on.
    /// </summary>
    public class ChannelTimedMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required string Message { get; set; }

        /// <summary>
        /// How long to wait between postings
        /// </summary>
        [Required]
        public required int IntervalMinutes { get; set; }

        /// <summary>
        /// The least chat messages that must have gone by since it last posted, so a quiet
        /// chat does not fill up with the bot talking to itself
        /// </summary>
        [Required]
        public required int MinimumChatMessages { get; set; }

        [Required]
        public required bool Enabled { get; set; }

        /// <summary>
        /// Whether it only posts while the channel is live
        /// </summary>
        [Required]
        public required bool OnlyWhenLive { get; set; }

        public DateTime? LastSentAt { get; set; }
    }
}
