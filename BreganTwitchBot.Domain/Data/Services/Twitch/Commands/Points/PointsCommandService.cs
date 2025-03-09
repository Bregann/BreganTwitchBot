using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
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

                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, $"{pointsResponse.TwitchUsername} has {pointsResponse.Points} {pointsName ?? "points"}", msgParams.MessageId);
                }
                catch (KeyNotFoundException)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "You silly sausage! That user doesn't exist", msgParams.MessageId);
                }
            }
        }
    }
}
