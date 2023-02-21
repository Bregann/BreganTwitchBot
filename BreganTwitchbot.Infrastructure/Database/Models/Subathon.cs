using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Subathon
    {
        [Key]
        public required string Username { get; set; }

        public required int SubsGifted { get; set; }

        public required int BitsDonated { get; set; }
    }
}