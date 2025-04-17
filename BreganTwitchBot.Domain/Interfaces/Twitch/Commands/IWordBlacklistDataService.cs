using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IWordBlacklistDataService
    {
        Task<string> HandleAddWordCommand(ChannelChatMessageReceivedParams msgParams, WordType wordType);
        Task<string> HandleRemoveWordCommand(ChannelChatMessageReceivedParams msgParams, WordType wordType);
    }
}
