using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ITwitchBossesDataService
    {
        Task StartBossFight(string broadcasterId, string broadcasterName);
        string HandleBossCommand(ChannelChatMessageReceivedParams msgParams);
        Task<bool> StartBossFightCountdown(string broadcasterId, string broadcasterName, ChannelChatMessageReceivedParams? msgParams = null);
    }
}
