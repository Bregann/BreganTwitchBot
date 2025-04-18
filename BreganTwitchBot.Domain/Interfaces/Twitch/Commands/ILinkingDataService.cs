using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ILinkingDataService
    {
        Task<string?> HandleLinkCommand(ChannelChatMessageReceivedParams msgParams);
    }
}
