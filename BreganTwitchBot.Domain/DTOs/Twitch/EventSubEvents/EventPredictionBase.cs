namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class EventPredictionBase
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string PredictionTitle { get; set; }
        public required string PredictionId { get; set; }
        public required string PredictionStatus { get; set; }

    }
}
