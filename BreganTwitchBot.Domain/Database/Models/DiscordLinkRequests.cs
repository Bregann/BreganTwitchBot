using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    public class DiscordLinkRequests
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string TwitchUsername { get; set; }

        [Required]
        public required ulong DiscordUserId { get; set; }

        [Required]
        public required int TwitchLinkCode { get; set; }
    }
}
