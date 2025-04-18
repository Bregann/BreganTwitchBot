namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelChatMessageReceivedParams : EventBase
    {
        public required string Message { get; set; }
        public required string[] MessageParts { get; set; }
        public required string MessageId { get; set; }
        public required bool IsMod { get; set; }
        public required bool IsVip { get; set; }
        public required bool IsSub { get; set; }
        public required bool IsBroadcaster { get; set; }
    }
}
