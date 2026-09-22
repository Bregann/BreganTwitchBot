using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
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

        /// <summary>
        /// When the current subathon was started. Null when one has never been run.
        /// </summary>
        public DateTime? SubathonStartTime { get; set; } = null;

        [Required]
        public required bool BroadcasterLive { get; set; }

        public required bool DiscordEnabled { get; set; } = false;

        // Discord related properties

        /// <summary>
        /// The ID of the Discord guild (server) that this channel is linked to.
        /// </summary>
        public ulong? DiscordGuildId { get; set; } = null;

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
        /// The ID of the Discord channel where general messages are sent.
        /// </summary>
        public ulong? DiscordGeneralChannelId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord role that has moderator permissions.
        /// </summary>
        public ulong? DiscordModeratorRoleId { get; set; } = null;

        /// <summary>
        /// The role /mute applies. The old bot looked this up by the name "mute", which broke
        /// if it was renamed and could not work across guilds.
        /// </summary>
        public ulong? DiscordMuteRoleId { get; set; } = null;

        /// <summary>
        /// The ID of the Discord channel where welcome messages are sent.
        /// </summary>
        public ulong? DiscordWelcomeMessageChannelId { get; set; } = null;

        /// <summary>
        /// The welcome message for somebody who has already linked their Twitch account.
        /// {user} is the new member, {twitchusername} their linked name and {commandschannel}
        /// the commands channel. Null falls back to the built in wording.
        /// </summary>
        public string? DiscordWelcomeMessageLinked { get; set; } = null;

        /// <summary>
        /// The welcome message for somebody who has not linked their Twitch account.
        /// {user} and {commandschannel} are replaced. Null falls back to the built in wording.
        /// </summary>
        public string? DiscordWelcomeMessageUnlinked { get; set; } = null;
    }
}
