using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ILeaderboardsDataService
    {
        Task<string> HandleLeaderboardCommand(ChannelChatMessageReceivedParams msgParams, LeaderboardType type);
    }
}
