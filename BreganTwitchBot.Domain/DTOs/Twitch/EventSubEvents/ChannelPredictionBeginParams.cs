
namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelPredictionBeginParams : EventPredictionBase
    {
        public required PredictionOutcomeOption[] PredictionOutcomeOptions { get; set; }
    }

    public class PredictionOutcomeOption
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
    }
}
