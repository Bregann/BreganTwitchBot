using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Subathon
    {
        [Key]
        [Required]
        public string Username { get; set; }

        [Required]
        public int SubsGifted { get; set; }

        [Required]
        public int BitsDonated { get; set; }
    }
}