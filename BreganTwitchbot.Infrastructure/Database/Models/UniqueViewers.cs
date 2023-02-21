using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class UniqueViewers
    {
        [Key]
        public required string Username { get; set; }
    }
}