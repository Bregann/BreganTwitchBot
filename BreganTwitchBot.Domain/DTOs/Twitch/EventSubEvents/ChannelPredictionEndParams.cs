namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelPredictionEndParams : EventPredictionBase
    {
        public required Outcome WonOutcome { get; set; }

        public class Outcome
        {
            public required string Id { get; set; }
            public required string Title { get; set; }
            public required int Users { get; set; }
            public required int ChannelPoints { get; set; }
            public required TopPredictor[] TopPredictors { get; set; }
        }

        public class TopPredictor
        {
            public required string UserName { get; set; }
            public required string UserId { get; set; }
            public required int? ChannelPointsWon { get; set; }
            public required int ChannelPointsUsed { get; set; }
        }
    }
}
