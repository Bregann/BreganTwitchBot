using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Subathon
{
    public class SubathonCommandService(IServiceProvider serviceProvider)
    {
        private readonly CommandCooldownHelper _subathonCooldown = new(TimeSpan.FromSeconds(5));

        [TwitchCommand("subathon")]
        public async Task SubathonCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var subathonDataService = scope.ServiceProvider.GetRequiredService<ISubathonDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                if (!isSuperMod && _subathonCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
                {
                    return;
                }

                _subathonCooldown.StartCooldown(msgParams.BroadcasterChannelId);

                var response = await subathonDataService.GetSubathonStatusAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
        }

        [TwitchCommand("startsubathon")]
        public async Task StartSubathonCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await RunModeratorCommand(msgParams, (service, p) => service.StartSubathonAsync(p));
        }

        [TwitchCommand("stopsubathon", ["endsubathon"])]
        public async Task StopSubathonCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await RunModeratorCommand(msgParams, (service, p) => service.StopSubathonAsync(p));
        }

        [TwitchCommand("addsubathontime", ["addsubtime"])]
        public async Task AddSubathonTimeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await RunModeratorCommand(msgParams, (service, p) => service.AddTimeManuallyAsync(p));
        }

        private async Task RunModeratorCommand(ChannelChatMessageReceivedParams msgParams, Func<ISubathonDataService, ChannelChatMessageReceivedParams, Task<string>> action)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var subathonDataService = scope.ServiceProvider.GetRequiredService<ISubathonDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                try
                {
                    var response = await action(subathonDataService, msgParams);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
                }
                catch (Exception ex)
                when (
                    ex is UnauthorizedAccessException ||
                    ex is InvalidCommandException
                )
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
            }
        }
    }
}
