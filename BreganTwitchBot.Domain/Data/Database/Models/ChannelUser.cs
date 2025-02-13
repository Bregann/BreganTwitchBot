using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelUser
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string TwitchUserId { get; set; }

        [Required]
        public required string TwitchUsername { get; set; }

        [Required]
        public required ulong DiscordUserId { get; set; } = 0;

        [Required]
        public required DateTime AddedOn { get; set; }
    }
}
