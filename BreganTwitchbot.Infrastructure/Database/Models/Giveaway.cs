﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class DiscordGiveaways
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string GiveawayId { get; set; }
        public required bool EligbleToWin { get; set; }
        public required ulong DiscordUserId { get; set; }
    }
}
