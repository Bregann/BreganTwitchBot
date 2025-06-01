using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge
{
    public class FollowAgeCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("followage", ["followtime", "howlong"])]
        public async Task FollowAgeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                var followAgeDataService = scope.ServiceProvider.GetRequiredService<IFollowAgeDataService>();
                var followAgeResponse = await followAgeDataService.HandleFollowCommandAsync(msgParams, Enums.FollowCommandTypeEnum.FollowAge);

                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, followAgeResponse, msgParams.MessageId);
            }
        }

        [TwitchCommand("followsince", ["followdate", "followedon"])]
        public async Task FollowSinceCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                var followAgeDataService = scope.ServiceProvider.GetRequiredService<IFollowAgeDataService>();
                var followAgeResponse = await followAgeDataService.HandleFollowCommandAsync(msgParams, Enums.FollowCommandTypeEnum.FollowSince);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, followAgeResponse, msgParams.MessageId);
            }
        }

        [TwitchCommand("followminutes", ["followmins"])]
        public async Task FollowMinutesCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                var followAgeDataService = scope.ServiceProvider.GetRequiredService<IFollowAgeDataService>();
                var followAgeResponse = await followAgeDataService.HandleFollowCommandAsync(msgParams, Enums.FollowCommandTypeEnum.FollowMinutes);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, followAgeResponse, msgParams.MessageId);
            }
        }
    }
}
