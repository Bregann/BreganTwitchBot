using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelUserData
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
        public required bool InStream { get; set; }

        [Required]
        public required bool IsSub { get; set; }

        [Required]
        public required bool IsSuperMod { get; set; }

        [Required]
        public required int TimeoutStrikes { get; set; }

        [Required]
        public required int WarnStrikes { get; set; }
    }
}
