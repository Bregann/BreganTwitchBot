using BreganTwitchBot.Domain.DTOs.Discord.Events;

namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordEventHelperService
    {
        Task HandleUserJoinedEvent(EventBase userJoined);
        Task HandleUserLeftEvent(EventBase userLeft);
        Task HandleMessageDeletedEvent(MessageDeletedEvent messageDeletedEvent);
        Task HandleMessageReceivedEvent(MessageReceivedEvent messageReceivedEvent);
        Task<(string MessageToSend, bool Ephemeral)> HandleButtonPressEvent(ButtonPressedEvent buttonPressedEvent);
    }
}
