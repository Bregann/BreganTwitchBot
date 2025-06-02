using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IHoursDataService
    {
        Task UpdateWatchtimeForChannel(string broadcasterId);
        Task<string> GetHoursCommand(ChannelChatMessageReceivedParams msgParams, HoursWatchTypes hoursType);
        Task ResetMinutes();
        Task ResetStreamMinutesForBroadcaster(string broadcasterId);
    }
}
