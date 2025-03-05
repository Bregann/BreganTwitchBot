using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
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

        // These sections are for the bot details
        [Required]
        public required string BotTwitchChannelName { get; set; }

        [Required]
        public required string BotTwitchChannelId { get; set; }

        [Required]
        public required string BotTwitchChannelOAuthToken { get; set; }

        [Required]
        public required string BotTwitchChannelRefreshToken { get; set; }

        public string? DiscordGuildId { get; set; } = null;
        public string? DiscordApiKey { get; set; } = null;

        public virtual ChannelConfig ChannelConfig { get; set; } = null!;
        public virtual ICollection<ChannelRank> ChannelRanks { get; set; } = null!;
        public virtual ICollection<CustomCommand> CustomCommands { get; set; } = null!;
        public virtual ICollection<DiscordLinkRequests> DiscordLinksRequests { get; set; } = null!;
        public virtual DiscordSpinStats DiscordSpinStats { get; set; } = null!;
        public virtual ICollection<StreamViewCount> StreamViewCounts { get; set; } = null!;
        public virtual TwitchSlotMachineStats TwitchSlotMachineStats { get; set; } = null!;
        public virtual ICollection<TwitchStreamStats> TwitchStreamStats { get; set; } = null!;
        public virtual ICollection<UniqueViewers> UniqueViewers { get; set; } = null!;
        public virtual ICollection<Subathon> Subathons { get; set; } = null!;
    }
}
