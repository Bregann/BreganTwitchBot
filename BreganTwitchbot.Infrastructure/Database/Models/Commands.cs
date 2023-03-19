using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Commands
    {
        [Key]
        public required string CommandName { get; set; }

        public required string CommandText { get; set; }

        public required DateTime LastUsed { get; set; }

        public required int TimesUsed { get; set; }
    }
}