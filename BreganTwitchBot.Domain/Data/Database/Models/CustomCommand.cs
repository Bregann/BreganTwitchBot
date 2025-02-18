using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class CustomCommand
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }

        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required string CommandName { get; set; }

        [Required]
        public required string CommandText { get; set; }

        [Required]
        public required DateTime LastUsed { get; set; }

        [Required]
        public required int TimesUsed { get; set; }
    }
}
