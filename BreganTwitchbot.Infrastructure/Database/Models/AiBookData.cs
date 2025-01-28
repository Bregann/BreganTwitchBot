using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class AiBookData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("User")]
        [Required]
        public string TwitchUserId { get; set; }
        public Users User { get; set; }

        public AiType AiType { get; set; }
        public string Value { get; set; }
    }

    public enum AiType
    {
        Book,
        Author,
        Series,
        Genre
    }
}
