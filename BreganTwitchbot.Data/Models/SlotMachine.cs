using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Data.Models
{
    public class SlotMachine
    {
        [Key]
        [Column("StreamName")]
        public string StreamName { get; set; }

        [Column("tier1Wins")]
        public int Tier1Wins { get; set; }

        [Column("tier2Wins")]
        public int Tier2Wins { get; set; }

        [Column("tier3Wins")]
        public int Tier3Wins { get; set; }

        [Column("jackpotWins")]
        public int JackpotWins { get; set; }

        [Column("totalSpins")]
        public int TotalSpins { get; set; }

        [Column("smorcWins")]
        public int SmorcWins { get; set; }

        [Column("jackpotAmount")]
        public long JackpotAmount { get; set; }

        [Column("grapesWins")]
        public int GrapesWins { get; set; }

        [Column("pineappleWins")]
        public int PineappleWins { get; set; }

        [Column("cherriesWins")]
        public int CherriesWins { get; set; }

        [Column("cucumberWins")]
        public int CucumberWins { get; set; }

        [Column("eggplantWins")]
        public int EggplantWins { get; set; }

        [Column("cheeseWins")]
        public int CheeseWins { get; set; }

        [Column("discordTotalSpins")]
        public int DiscordTotalSpins { get; set; }
    }
}
