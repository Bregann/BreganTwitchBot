﻿using BreganTwitchBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    public class Blacklist
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string Word { get; set; }

        [Required]
        public required WordType WordType { get; set; }

        [Required]
        [ForeignKey(nameof(ChannelId))]
        public required int ChannelId { get; set; }
        virtual public Channel Channel { get; set; } = null!;
    }
}
