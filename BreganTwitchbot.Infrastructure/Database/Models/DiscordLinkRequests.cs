using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class DiscordLinkRequests
    {
        [Key]
        public required string TwitchName { get; set; }

        public required ulong DiscordID { get; set; }
    }
}