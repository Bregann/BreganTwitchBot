using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class SlotMachine
    {
        [Key]
        public required string StreamName { get; set; }

        public required int Tier1Wins { get; set; }

        public required int Tier2Wins { get; set; }

        public required int Tier3Wins { get; set; }

        public required int JackpotWins { get; set; }

        public required int TotalSpins { get; set; }

        public required int SmorcWins { get; set; }

        public required long JackpotAmount { get; set; }

        public required int GrapesWins { get; set; }

        public required int PineappleWins { get; set; }

        public required int CherriesWins { get; set; }

        public required int CucumberWins { get; set; }

        public required int EggplantWins { get; set; }

        public required int CheeseWins { get; set; }

        public required int DiscordTotalSpins { get; set; }
    }
}