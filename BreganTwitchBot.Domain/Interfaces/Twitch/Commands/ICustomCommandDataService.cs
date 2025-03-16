using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ICustomCommandDataService
    {
        Task HandleCustomCommandAsync(string command, ChannelChatMessageReceivedParams msgParams);
        Task<string> AddNewCustomCommand(ChannelChatMessageReceivedParams msgParams);
        Task<string> EditCustomCommand(ChannelChatMessageReceivedParams msgParams);
        Task<string> DeleteCustomCommand(ChannelChatMessageReceivedParams msgParams);
    }
}
