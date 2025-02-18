using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class DiscordSpinStats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required int GrapesWins { get; set; }

        [Required]
        public required int PineappleWins { get; set; }

        [Required]
        public required int CherriesWins { get; set; }

        [Required]
        public required int CucumberWins { get; set; }

        [Required]
        public required int EggplantWins { get; set; }

        [Required]
        public required int CheeseWins { get; set; }

        [Required]
        public required int DiscordTotalSpins { get; set; }
    }
}
