using BreganTwitchBot.Domain.DTOs.Twitch.Commands.Points;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IPointsDataService
    {
        Task<string> GetPointsAsync(ChannelChatMessageReceivedParams msgParams);
        Task AddPointsAsync(ChannelChatMessageReceivedParams msgParams);
    }
}
