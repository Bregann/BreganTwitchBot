using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Points
{
    public class PointsCommandService(IPointsDataService pointsDataService, ITwitchHelperService twitchHelperService)
    {
        [TwitchCommand("points", ["pooants", "coins"])]
        public async Task PointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            try
            {
                var pointsResponse = await pointsDataService.GetPointsAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, pointsResponse, msgParams.MessageId);
            }
            catch (TwitchUserNotFoundException)
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "You silly sausage! That user doesn't exist", msgParams.MessageId);
            }
        }

        [TwitchCommand("addpoints", ["pointsadd", "addcoins", "coinsadd"])]
        public async Task AddPointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            try
            {
                await pointsDataService.AddPointsAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "Theeee points 'ave been added", msgParams.MessageId);
            }
            catch (InvalidCommandException ex)
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
            catch (TwitchUserNotFoundException)
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "You silly sausage! That user doesn't exist", msgParams.MessageId);
            }
        }
    }
}
