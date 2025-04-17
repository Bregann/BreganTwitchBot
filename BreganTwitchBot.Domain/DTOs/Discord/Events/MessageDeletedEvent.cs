namespace BreganTwitchBot.Domain.DTOs.Discord.Events
{
    public class MessageDeletedEvent : EventBase
    {
        public required ulong ChannelId { get; set; }
        public required string ChannelName { get; set; }
        public required ulong MessageId { get; set; }
        public required string MessageContent { get; set; }
    }
}
