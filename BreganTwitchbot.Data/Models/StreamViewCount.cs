using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchbot.Data.Models
{
    public class StreamViewCount
    {
        [Key]
        [Column("Time")]
        public DateTime Time { get; set; }

        [Column("ViewCount")]
        public int ViewCount { get; set; }
    }
}
