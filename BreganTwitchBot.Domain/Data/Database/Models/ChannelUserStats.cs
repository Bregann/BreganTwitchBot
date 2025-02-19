using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelUserStats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public int ChannelUserId { get; set; }
        public virtual ChannelUser User { get; set; } = null!;

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required int TotalMessages { get; set; }

        [Required]
        public required int GiftedSubsThisMonth { get; set; }

        [Required]
        public required int BitsDonatedThisMonth { get; set; }

        [Required]
        public required int MarblesWins { get; set; }

        [Required]
        public required int BossesDone { get; set; }

        [Required]
        public required long BossesPointsWon { get; set; }
    }
}
