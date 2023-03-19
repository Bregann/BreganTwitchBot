using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class UserGambleStats
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Users User { get; set; }
        public string TwitchUserId { get; set; }

        public required long PointsGambled { get; set; }
        public required long PointsWon { get; set; }
        public required long PointsLost { get; set; }
        public required int TotalSpins { get; set; }
        public required int Tier1Wins { get; set; }
        public required int Tier2Wins { get; set; }
        public required int Tier3Wins { get; set; }
        public required int JackpotWins { get; set; }
        public required int SmorcWins { get; set; }
    }
}