using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points
{
    public class PointsCommandService(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [TwitchCommand("points", ["pooants", "tilly"])]
        public async Task PointsCommand(ChannelChatMessageArgs context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pointsDataService = scope.ServiceProvider.GetRequiredService<IPointsDataService>();
                var points = await pointsDataService.GetPointsAsync(context.Notification.Payload.Event.ChatterUserId);
                
                // send a message blah blah blah
            }
        }
    }
}
