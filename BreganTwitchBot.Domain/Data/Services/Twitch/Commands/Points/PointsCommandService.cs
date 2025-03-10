using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points
{
    public class PointsCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("points", ["pooants", "coins"])]
        public async Task PointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var pointsDataService = scope.ServiceProvider.GetRequiredService<IPointsDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                try
                {
                    var pointsResponse = await pointsDataService.GetPointsAsync(msgParams);
                    var pointsName = await twitchHelperService.GetPointsName(msgParams.BroadcasterChannelName);

                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, $"{pointsResponse.TwitchUsername} has {pointsResponse.Points:N0} {pointsName ?? "points"}. Rank: {pointsResponse.Position}", msgParams.MessageId);
                }
                catch (TwitchUserNotFoundException)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "You silly sausage! That user doesn't exist", msgParams.MessageId);
                }
            }
        }

        [TwitchCommand("addpoints", ["pointsadd", "addcoins", "coinsadd"])]
        public async Task AddPointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using(var scope = serviceProvider.CreateScope())
            {
                var pointsDataService = scope.ServiceProvider.GetRequiredService<IPointsDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelName, msgParams.ChatterChannelName);

                if (!isSuperMod)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "You don't have permission to do that", msgParams.MessageId);
                    return;
                }

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
}
