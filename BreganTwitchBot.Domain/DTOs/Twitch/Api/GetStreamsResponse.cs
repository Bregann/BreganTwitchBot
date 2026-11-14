namespace BreganTwitchBot.Domain.DTOs.Twitch.Api
{
    public class GetStreamsResponse
    {
        /// <summary>
        /// Twitch's id for this broadcast
        /// </summary>
        public required string Id { get; set; }
        public required string GameId { get; set; }
        public required string GameName { get; set; }
        public required int ViewerCount { get; set; }
        public required DateTime StartedAt { get; set; }
    }
}
