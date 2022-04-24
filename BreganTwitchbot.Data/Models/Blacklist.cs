using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Data.Models
{
    public class Blacklist
    {
        [Key]
        [Column("Word")]
        public string Word { get; set; }

        [Column("WordType")]
        public string WordType { get; set; }
    }
}
