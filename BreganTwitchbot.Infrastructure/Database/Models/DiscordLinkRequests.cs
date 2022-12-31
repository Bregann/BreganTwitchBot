using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class DiscordLinkRequests
    {
        [Key]
        [Required]
        public string TwitchName { get; set; }

        [Required]
        public ulong DiscordID { get; set; }
    }
}