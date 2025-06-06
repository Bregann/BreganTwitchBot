﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    public class ChannelUser
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string TwitchUserId { get; set; }

        [Required]
        public required string TwitchUsername { get; set; }

        [Required]
        public required ulong DiscordUserId { get; set; } = 0;

        [Required]
        public required DateTime AddedOn { get; set; }

        [Required]
        public required DateTime LastSeen { get; set; }

        [Required]
        public required bool CanUseOpenAi { get; set; }

        public virtual ICollection<ChannelUserData> ChannelUserData { get; set; } = null!;
        public virtual ICollection<ChannelUserGambleStats> ChannelUserGambleStats { get; set; } = null!;
        public virtual ICollection<ChannelUserRankProgress> ChannelUserRankProgress { get; set; } = null!;
        public virtual ICollection<ChannelUserStats> ChannelUserStats { get; set; } = null!;
        public virtual ICollection<ChannelUserWatchtime> ChannelUserWatchtimes { get; set; } = null!;
        public virtual ICollection<DiscordDailyPoints> DiscordDailyPoints { get; set; } = null!;
        public virtual ICollection<DiscordUserStats> DiscordUserStats { get; set; } = null!;
        public virtual ICollection<TwitchDailyPoints> TwitchDailyPoints { get; set; } = null!;
        public virtual ICollection<Subathon> Subathons { get; set; } = null!;
    }
}
