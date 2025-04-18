using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class StreamViewCount
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;
        public DateTime Time { get; set; }

        [Required]
        public required int ViewCount { get; set; }
    }
}
