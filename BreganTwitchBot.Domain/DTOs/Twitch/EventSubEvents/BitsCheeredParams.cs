namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class BitsCheeredParams : EventBase
    {
        public required int Amount { get; set; }
        public required string Message { get; set; }
        public required bool IsAnonymous { get; set; }
    }
}
