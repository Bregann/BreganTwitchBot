using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class DiscordUserStats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public required int ChannelUserId { get; set; }
        virtual public ChannelUser User { get; set; } = null!;

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }
        virtual public Channel Channel { get; set; } = null!;

        [Required]
        public required int DiscordLevel { get; set; }

        [Required]
        public required int DiscordXp { get; set; }

        [Required]
        public required bool DiscordLevelUpNotifsEnabled { get; set; }

        [Required]
        public required int PrestigeLevel { get; set; }
    }
}
