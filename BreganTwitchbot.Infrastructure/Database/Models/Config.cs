using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Config
    {
        [Key]
        public required string BroadcasterName { get; set; }
        public required string BroadcasterOAuth { get; set; }
        public required string BroadcasterRefresh { get; set; }
        public required bool StreamAnnounced { get; set; }
        public required bool DailyPointsCollectingAllowed { get; set; }
        public required DateTime LastDailyPointsAllowed { get; set; }
        public required TimeSpan SubathonTime { get; set; }
        public required string BotName { get; set; }
        public required string PointsName { get; set; }
        public required string TwitchChannelID { get; set; }
        public required string BotOAuth { get; set; }
        public required string TwitchAPIClientID { get; set; }
        public required string TwitchAPISecret { get; set; }
        public required long PrestigeCap { get; set; }
        public required string HFConnectionString { get; set; }
        public required string ProjectMonitorApiKey { get; set; }
        public required string TwitchBotApiKey { get; set; }
        public required string TwitchBotApiRefresh { get; set; }
        public required string BotChannelId { get; set; }
        public required bool SubathonActive { get; set; }
        public required string HangfireUsername { get; set; }
        public required string HangfirePassword { get; set; }
    }
}