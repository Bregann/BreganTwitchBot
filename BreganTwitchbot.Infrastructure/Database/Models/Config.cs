using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Config
    {
        [Key]
        [Required]
        public string BroadcasterName { get; set; }

        [Required]
        public string BroadcasterOAuth { get; set; }

        [Required]
        public string BroadcasterRefresh { get; set; }

        [Required]
        public bool StreamAnnounced { get; set; }

        [Required]
        public bool DailyPointsCollectingAllowed { get; set; }

        [Required]
        public DateTime LastDailyPointsAllowed { get; set; }

        [Required]
        public TimeSpan SubathonTime { get; set; }

        [Required]
        public string PinnedStreamMessage { get; set; }

        [Required]
        public ulong PinnedStreamMessageId { get; set; }

        [Required]
        public DateTime PinnedStreamDate { get; set; }

        [Required]
        public string BotName { get; set; }

        [Required]
        public string PointsName { get; set; }

        [Required]
        public string TwitchChannelID { get; set; }

        [Required]
        public string BotOAuth { get; set; }

        [Required]
        public string TwitchAPIClientID { get; set; }

        [Required]
        public string TwitchAPISecret { get; set; }

        [Required]
        public string DiscordAPIKey { get; set; }

        [Required]
        public ulong DiscordGuildOwner { get; set; }

        [Required]
        public ulong DiscordEventChannelID { get; set; }

        [Required]
        public ulong DiscordStreamAnnouncementChannelID { get; set; }

        [Required]
        public ulong DiscordLinkingChannelID { get; set; }

        [Required]
        public ulong DiscordCommandsChannelID { get; set; }

        [Required]
        public ulong DiscordRankUpAnnouncementChannelID { get; set; }

        [Required]
        public ulong DiscordGiveawayChannelID { get; set; }

        [Required]
        public ulong DiscordSocksChannelID { get; set; }

        [Required]
        public ulong DiscordReactionRoleChannelID { get; set; }

        [Required]
        public ulong DiscordGeneralChannel { get; set; }

        [Required]
        public ulong DiscordGuildID { get; set; }

        [Required]
        public ulong DiscordBanRole { get; set; }

        [Required]
        public long PrestigeCap { get; set; }

        [Required]
        public string HFConnectionString { get; set; }

        [Required]
        public string ProjectMonitorApiKey { get; set; }

        [Required]
        public string TwitchBotApiKey { get; set; }

        [Required]
        public string TwitchBotApiRefresh { get; set; }
    }
}