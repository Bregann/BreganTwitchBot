using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IFollowAgeDataService
    {
        Task<string> HandleFollowCommandAsync(ChannelChatMessageReceivedParams msgParams, FollowCommandTypeEnum followCommandType);
    }
}
