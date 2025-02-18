using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class DiscordDailyPoints
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public int ChannelUserId { get; set; }

        public virtual ChannelUser User { get; set; } = null!;

        [Required]
        public required int DiscordDailyStreak { get; set; }

        [Required]
        public required int DiscordDailyTotalClaims { get; set; }

        [Required]
        public required bool DiscordDailyClaimed { get; set; }
    }
}
