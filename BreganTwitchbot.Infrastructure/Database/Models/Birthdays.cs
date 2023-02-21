using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Birthdays
    {
        [Key]
        public ulong DiscordId { get; set; }

        public required int Day { get; set; }

        public required int Month { get; set; }
    }
}