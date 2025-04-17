namespace BreganTwitchBot.Domain.DTOs.Discord.Events
{
    public class EventBase
    {
        public required ulong GuildId { get; set; }
        public required ulong UserId { get; set; }
        public required string Username { get; set; }
    }
}
