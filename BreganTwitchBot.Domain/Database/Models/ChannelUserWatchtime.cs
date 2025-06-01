using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    public class ChannelUserWatchtime
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public required int ChannelUserId { get; set; }
        public virtual ChannelUser ChannelUser { get; set; } = null!;

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required int MinutesInStream { get; set; }

        [Required]
        public required int MinutesWatchedThisStream { get; set; }

        [Required]
        public required int MinutesWatchedThisWeek { get; set; }

        [Required]
        public required int MinutesWatchedThisMonth { get; set; }

        [Required]
        public required int MinutesWatchedThisYear { get; set; }
    }
}
