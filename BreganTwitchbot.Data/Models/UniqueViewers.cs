using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchbot.Data.Models
{
    public class UniqueViewers
    {
        [Key]
        [Column("Username")]
        public string Username { get; set; }
    }
}
