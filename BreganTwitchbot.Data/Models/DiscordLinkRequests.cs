using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Data.Models
{
    public class DiscordLinkRequests
    {
        [Key]
        [Column("TwitchName")]
        public string TwitchName { get; set; }

        [Column("discordID")]
        public ulong DiscordID { get; set; }
    }
}
