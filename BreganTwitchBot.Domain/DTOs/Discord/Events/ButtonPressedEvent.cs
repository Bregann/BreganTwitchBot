namespace BreganTwitchBot.Domain.DTOs.Discord.Events
{
    public class ButtonPressedEvent : EventBase
    {
        public required string CustomId { get; set; }
    }
}
