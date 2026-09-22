namespace BreganTwitchBot.Domain.DTOs.Discord.Events
{
    public class MessageReceivedEvent : EventBase
    {
        public required ulong ChannelId { get; set; }
        public required string ChannelName { get; set; }
        public required ulong MessageId { get; set; }
        public required string MessageContent { get; set; }
        public required bool HasAttachments { get; set; }

        /// <summary>
        /// Whether the author holds the guild's configured moderator role. Resolved at the
        /// point the message arrives, where the guild user is available.
        /// </summary>
        public required bool AuthorIsMod { get; set; }
    }
}
