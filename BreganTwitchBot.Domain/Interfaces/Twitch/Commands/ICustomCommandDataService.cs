﻿using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ICustomCommandDataService
    {
        Task HandleCustomCommandAsync(string command, ChannelChatMessageReceivedParams msgParams);
        Task<string> AddNewCustomCommandAsync(ChannelChatMessageReceivedParams msgParams);
        Task<string> EditCustomCommandAsync(ChannelChatMessageReceivedParams msgParams);
        Task<string> DeleteCustomCommandAsync(ChannelChatMessageReceivedParams msgParams);
    }
}
