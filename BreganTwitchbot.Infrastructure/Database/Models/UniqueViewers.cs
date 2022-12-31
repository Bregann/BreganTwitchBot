using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class UniqueViewers
    {
        [Key]
        [Required]
        public string Username { get; set; }
    }
}