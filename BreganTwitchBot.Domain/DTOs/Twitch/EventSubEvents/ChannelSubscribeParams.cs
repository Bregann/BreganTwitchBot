using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelSubscribeParams : EventBase
    {
        public required SubTierEnum SubTier { get; set; }
        public required bool IsGift { get; set; }
    }
}
