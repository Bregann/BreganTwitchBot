using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Gambling
{
    public class GamblingCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("spin", ["slots"])]
        public async Task HandleSpinCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var gamblingDataService = scope.ServiceProvider.GetRequiredService<IGamblingDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                try
                {
                    var spinResponse = await gamblingDataService.HandleSpinCommand(msgParams);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, spinResponse, msgParams.MessageId);
                }
                catch (InvalidCommandException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
            }
        }

        [TwitchCommand("jackpot", ["jackpotamount", "yackpot"])]
        public async Task HandleJackpotCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var gamblingDataService = scope.ServiceProvider.GetRequiredService<IGamblingDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var jackpotResponse = await gamblingDataService.GetJackpotAmount(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, jackpotResponse, msgParams.MessageId);
            }
        }
    }
}
