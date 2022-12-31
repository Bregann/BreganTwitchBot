using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Commands
    {
        [Key]
        [Required]
        public string CommandName { get; set; }

        [Required]
        public string CommandText { get; set; }

        [Required]
        public DateTime LastUsed { get; set; }

        [Required]
        public int TimesUsed { get; set; }
    }
}