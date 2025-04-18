using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelUserRankProgress
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public required int ChannelUserId { get; set; }
        virtual public ChannelUser ChannelUser { get; set; } = null!;

        [ForeignKey(nameof(Channel))]
        public required int ChannelId { get; set; }
        virtual public Channel Channel { get; set; } = null!;

        [ForeignKey(nameof(ChannelRank))]
        [Required]
        public required int ChannelRankId { get; set; }
        virtual public ChannelRank ChannelRank { get; set; } = null!;

        [Required]
        public required DateTime AchievedAt { get; set; }
    }
}
