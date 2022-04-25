using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Data.Models
{
    public class Subathon
    {
        [Key]
        [Column("username")]
        public string Username { get; set; }

        [Column("subsGifted")]
        public int SubsGifted { get; set; }

        [Column("bitsDonated")]
        public int BitsDonated { get; set; }
    }
}
