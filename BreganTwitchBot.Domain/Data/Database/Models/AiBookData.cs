using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class AiBookData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        [Required]
        public int ChannelUserId { get; set; }

        public virtual ChannelUser User { get; set; } = null!;

        [Required]
        public required AiType AiType { get; set; }

        [Required]
        public required string Value { get; set; }
    }
}
