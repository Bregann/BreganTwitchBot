using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
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
    }
}
