using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge
{
    public class FollowAgeCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("followage", ["followtime"])]
        public async Task FollowAgeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                var followAgeDataService = scope.ServiceProvider.GetRequiredService<IFollowAgeDataService>();
                var followAgeResponse = await followAgeDataService.GetFollowAgeAsync(msgParams);

                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, followAgeResponse, msgParams.MessageId);
            }
        }
    }
}
