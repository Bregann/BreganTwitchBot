namespace BreganTwitchBot.Domain.DTOs.Twitch.Api
{
    public class GetChannelInformationResponse
    {
        public required string Title { get; set; }
        public required string GameId { get; set; }
        public required string GameName { get; set; }
    }
}
