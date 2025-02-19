﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelUserRankProgress
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(ChannelUser))]
        [Required]
        public required int ChannelUserId { get; set; }
        virtual public ChannelUser ChannelUser { get; set; } = null!;

        [ForeignKey(nameof(ChannelRank))]
        [Required]
        public required int ChannelRankId { get; set; }
        virtual public ChannelRank ChannelRank { get; set; } = null!;

        [Required]
        public required DateTime AchievedAt { get; set; }
    }
}
