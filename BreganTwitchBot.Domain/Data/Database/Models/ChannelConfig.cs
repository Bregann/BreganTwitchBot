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

        [Required]
        public required bool BroadcasterLive { get; set; }

        // Discord related properties

        /// <summary>
        /// The ID of the Discord guild (server) that this channel is linked to.
        /// </summary>
        public ulong? DiscordGuildOwnerId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord channel where Discord events are sent.
        /// </summary>
        public ulong? DiscordEventChannelId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord channel where stream announcements are sent.
        /// </summary>
        public ulong? DiscordStreamAnnouncementChannelId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord channel where user commands are sent.
        /// </summary>
        public ulong? DiscordUserCommandsChannelId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord channel where user rank-up announcements are sent.
        /// </summary>
        public ulong? DiscordUserRankUpAnnouncementChannelId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord channel where giveaways are sent.
        /// </summary>
        public ulong? DiscordGiveawayChannelId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord role that has ban permissions.
        /// </summary>
        public ulong? DiscordBanRoleChannelId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord role that has moderator permissions.
        /// </summary>
        public ulong? DiscordModeratorRoleId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord channel where welcome messages are sent.
        /// </summary>
        public ulong? DiscordWelcomeMessageChannelId { get; set; } = null;
    }
}
