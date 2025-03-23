using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelConfig
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Channel))]
        public int ChannelId { get; set; }

        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required string ChannelCurrencyName { get; set; }

        [Required]
        public required long CurrencyPointCap { get; set; }

        [Required]
        public required bool StreamAnnounced { get; set; }

        [Required]
        public required bool StreamHappenedThisWeek { get; set; }

        [Required]
        public required bool DailyPointsCollectingAllowed { get; set; }

        [Required]
        public required DateTime LastDailyPointsAllowed { get; set; }

        [Required]
        public required DateTime LastStreamStartDate { get; set; }

        [Required]
        public required DateTime LastStreamEndDate { get; set; }

        [Required]
        public required bool SubathonActive { get; set; }

        [Required]
        public required TimeSpan SubathonTime { get; set; }

        // Discord related properties
        public ulong? DiscordGuildOwnerId { get; set; } = null;
        public ulong? DiscordEventChannelId { get; set; } = null;
        public ulong? DiscordStreamAnnouncementChannelId { get; set; } = null;
        public ulong? DiscordUserLinkingChannelId { get; set; } = null;
        public ulong? DiscordUserCommandsChannelId { get; set; } = null;
        public ulong? DiscordUserRankUpAnnouncementChannelId { get; set; } = null;
        public ulong? DiscordGiveawayChannelId { get; set; } = null;
        public ulong? DiscordReactionRoleChannelId { get; set; } = null;
        public ulong? DiscordGeneralChannelId { get; set; } = null;
        public ulong? DiscordBanRoleChannelId { get; set; } = null;
    }
}
