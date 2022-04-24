using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Data.Models
{
    public class Commands
    {
        [Key]
        [Column("commandName")]
        public string CommandName { get; set; }

        [Column("commandText")]
        public string CommandText { get; set; }

        [Column("lastUsed")]
        public DateTime LastUsed { get; set; }

        [Column("timesUsed")]
        public int TimesUsed { get; set; }
    }
}
