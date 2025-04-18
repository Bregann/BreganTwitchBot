using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelResubscribeParams : EventBase
    {
        public required string Message { get; set; }
        public int? StreakMonths { get; set; }
        public required int CumulativeMonths { get; set; }
        public required SubTierEnum SubTier { get; set; }
    }
}
