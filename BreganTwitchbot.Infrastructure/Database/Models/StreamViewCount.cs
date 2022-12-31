using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class StreamViewCount
    {
        [Key]
        [Required]
        public DateTime Time { get; set; }

        [Required]
        public int ViewCount { get; set; }
    }
}