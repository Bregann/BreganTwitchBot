using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class DiscordLinkRequests
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }

        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required string TwitchUserId { get; set; }

        [Required]
        public required string DiscordUserId { get; set; }
    }
}
