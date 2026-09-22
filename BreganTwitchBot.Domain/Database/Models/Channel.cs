using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    public class Channel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // These sections are for the actual streamer details
        [Required]
        public required string BroadcasterTwitchChannelName { get; set; }

        [Required]
        public required string BroadcasterTwitchChannelId { get; set; }

        [Required]
        public required string BroadcasterTwitchChannelOAuthToken { get; set; }

        [Required]
        public required string BroadcasterTwitchChannelRefreshToken { get; set; }

        // The bot account is no longer per channel - a single bot connects to every
        // channel, and its credentials live in the EnvironmentalSettings table.

        public virtual ChannelConfig ChannelConfig { get; set; } = null!;
        public virtual ICollection<ChannelRank> ChannelRanks { get; set; } = null!;
        public virtual ICollection<CustomCommand> CustomCommands { get; set; } = null!;
        public virtual ICollection<ChannelBossText> ChannelBossTexts { get; set; } = null!;
        public virtual ICollection<ChannelPointReward> ChannelPointRewards { get; set; } = null!;
        public virtual ICollection<DiscordGiveaway> DiscordGiveaways { get; set; } = null!;
        public virtual DiscordGiveawayConfig DiscordGiveawayConfig { get; set; } = null!;
        public virtual ICollection<MonthlyLeaderboardRole> MonthlyLeaderboardRoles { get; set; } = null!;
        public virtual ICollection<DiscordSelfAssignRole> DiscordSelfAssignRoles { get; set; } = null!;
        public virtual DiscordSpinStats DiscordSpinStats { get; set; } = null!;
        public virtual ICollection<StreamViewCount> StreamViewCounts { get; set; } = null!;
        public virtual TwitchSlotMachineStats TwitchSlotMachineStats { get; set; } = null!;
        public virtual ICollection<TwitchStreamStats> TwitchStreamStats { get; set; } = null!;
        public virtual ICollection<UniqueViewers> UniqueViewers { get; set; } = null!;
        public virtual ICollection<Subathon> Subathons { get; set; } = null!;
        public virtual ICollection<SubathonRate> SubathonRates { get; set; } = null!;
    }
}
