using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ICommandHandler
    {
        void RegisterCommands();
        void LoadCustomCommands();
        Task HandleCommandAsync(string command, ChannelChatMessageReceivedParams msgParams);
        bool IsSystemCommand(string commandName);
        bool RemoveCustomCommand(string commandName, string broadcasterId);
        void AddCustomCommand(string commandName, string broadcasterId);
    }
}
