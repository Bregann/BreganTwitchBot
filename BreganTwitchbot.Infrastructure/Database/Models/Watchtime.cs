using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Watchtime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Users User { get; set; }

        public string TwitchUserId { get; set; }

        public required int MinutesInStream { get; set; }

        public required int MinutesWatchedThisStream { get; set; }

        public required int MinutesWatchedThisWeek { get; set; }

        public required int MinutesWatchedThisMonth { get; set; }

        public required bool Rank1Applied { get; set; }

        public required bool Rank2Applied { get; set; }

        public required bool Rank3Applied { get; set; }

        public required bool Rank4Applied { get; set; }

        public required bool Rank5Applied { get; set; }
    }
}