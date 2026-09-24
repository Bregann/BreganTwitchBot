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

        /// <summary>
        /// Every built in command with its aliases, so the website can list them from
        /// the live registry rather than a hardcoded page that goes stale
        /// </summary>
        IReadOnlyList<(string CommandName, string[] Aliases)> GetRegisteredCommands();
    }
}
