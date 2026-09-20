using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.FollowAge
{
    public class FollowAgeCommandService(IFollowAgeDataService followAgeDataService, ITwitchHelperService twitchHelperService)
    {
        [TwitchCommand("followage", ["followtime", "howlong", "followerage"])]
        public async Task FollowAgeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var followAgeResponse = await followAgeDataService.HandleFollowCommandAsync(msgParams, Enums.FollowCommandTypeEnum.FollowAge);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, followAgeResponse, msgParams.MessageId);
        }

        [TwitchCommand("followsince", ["followdate", "followedon"])]
        public async Task FollowSinceCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var followAgeResponse = await followAgeDataService.HandleFollowCommandAsync(msgParams, Enums.FollowCommandTypeEnum.FollowSince);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, followAgeResponse, msgParams.MessageId);
        }

        [TwitchCommand("followminutes", ["followmins"])]
        public async Task FollowMinutesCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var followAgeResponse = await followAgeDataService.HandleFollowCommandAsync(msgParams, Enums.FollowCommandTypeEnum.FollowMinutes);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, followAgeResponse, msgParams.MessageId);
        }
    }
}
