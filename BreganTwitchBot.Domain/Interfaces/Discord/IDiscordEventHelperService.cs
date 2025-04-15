using BreganTwitchBot.Domain.DTOs.Discord.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
