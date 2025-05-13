namespace BreganTwitchBot.Domain.DTOs.Twitch.Api
{
    public class GetStreamsResponse
    {
        public required string GameId { get; set; }
        public required string GameName { get; set; }
        public required int ViewerCount { get; set; }
        public required DateTime StartedAt { get; set; }
    }
}
