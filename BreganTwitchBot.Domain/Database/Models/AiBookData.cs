using BreganTwitchBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    public class AiBookData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        [Required]
        public int ChannelUserId { get; set; }

        public virtual ChannelUser User { get; set; } = null!;

        [Required]
        public required AiType AiType { get; set; }

        [Required]
        public required string Value { get; set; }
    }
}
