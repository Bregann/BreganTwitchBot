using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class StreamViewCount
    {
        [Key]
        public required DateTime Time { get; set; }

        public required int ViewCount { get; set; }
    }
}