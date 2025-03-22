using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
