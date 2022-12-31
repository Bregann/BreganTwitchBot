using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Birthdays
    {
        [Key]
        [Required]
        public ulong DiscordId { get; set; }

        [Required]
        public int Day { get; set; }

        [Required]
        public int Month { get; set; }
    }
}