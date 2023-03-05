using BreganTwitchBot.Infrastructure.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Blacklist
    {
        [Key]
        public required string Word { get; set; }

        public required WordTypes WordType { get; set; }
    }
}