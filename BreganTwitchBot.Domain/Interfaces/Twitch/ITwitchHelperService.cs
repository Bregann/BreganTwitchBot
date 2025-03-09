using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITwitchHelperService
    {
        Task SendTwitchMessageToChannel(string broadcasterChannelId, string broadcasterChannelName, string message, string? originalMessageId = null);
        Task<string?> GetTwitchUserIdFromUsername(string username);
    }
}
