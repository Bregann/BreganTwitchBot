using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IFollowAgeDataService
    {
        Task<string> HandleFollowCommandAsync(ChannelChatMessageReceivedParams msgParams, FollowCommandTypeEnum followCommandType);
    }
}
