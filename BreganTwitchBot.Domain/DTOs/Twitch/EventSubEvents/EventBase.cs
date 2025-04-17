namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class EventBase
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string ChatterChannelName { get; set; }
        public required string ChatterChannelId { get; set; }
    }
}
