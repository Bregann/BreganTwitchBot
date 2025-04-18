using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ITwitchBossesDataService
    {
        Task StartBossFight(string broadcasterId, string broadcasterName);
        string HandleBossCommand(ChannelChatMessageReceivedParams msgParams);
        Task<bool> StartBossFightCountdown(string broadcasterId, string broadcasterName, ChannelChatMessageReceivedParams? msgParams = null);
    }
}
