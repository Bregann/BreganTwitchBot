using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Blacklist
    {
        [Key]
        [Required]
        public string Word { get; set; }

        [Required]
        public string WordType { get; set; }
    }
}