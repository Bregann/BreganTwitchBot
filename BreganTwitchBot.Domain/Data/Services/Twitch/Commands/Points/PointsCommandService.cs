using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points
{
    public class PointsCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("points", ["pooants", "tilly"])]
        public async Task PointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var pointsDataService = scope.ServiceProvider.GetRequiredService<IPointsDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var points = await pointsDataService.GetPointsAsync(msgParams.ChatterChannelId);

                // send a message blah blah blah
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "testing points", msgParams.MessageId);
            }
        }
    }
}
