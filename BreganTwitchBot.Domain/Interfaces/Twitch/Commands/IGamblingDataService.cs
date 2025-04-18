using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IGamblingDataService
    {
        Task<string> HandleSpinCommand(ChannelChatMessageReceivedParams msgParams);
        Task<string> GetJackpotAmount(ChannelChatMessageReceivedParams msgParams);
    }
}
