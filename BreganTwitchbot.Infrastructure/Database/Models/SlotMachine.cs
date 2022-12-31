using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class SlotMachine
    {
        [Key]
        [Required]
        public string StreamName { get; set; }

        [Required]
        public int Tier1Wins { get; set; }

        [Required]
        public int Tier2Wins { get; set; }

        [Required]
        public int Tier3Wins { get; set; }

        [Required]
        public int JackpotWins { get; set; }

        [Required]
        public int TotalSpins { get; set; }

        [Required]
        public int SmorcWins { get; set; }

        [Required]
        public long JackpotAmount { get; set; }

        [Required]
        public int GrapesWins { get; set; }

        [Required]
        public int PineappleWins { get; set; }

        [Required]
        public int CherriesWins { get; set; }

        [Required]
        public int CucumberWins { get; set; }

        [Required]
        public int EggplantWins { get; set; }

        [Required]
        public int CheeseWins { get; set; }

        [Required]
        public int DiscordTotalSpins { get; set; }
    }
}