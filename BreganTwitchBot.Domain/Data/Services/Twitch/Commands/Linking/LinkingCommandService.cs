using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Linking
{
    public class LinkingCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("link")]
        public async Task HandleLinkCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var linkingDataService = scope.ServiceProvider.GetRequiredService<ILinkingDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                try
                {
                    var response = await linkingDataService.HandleLinkCommand(msgParams);

                    if (response == null)
                    {
                        Log.Information("Link command not sent, returned null");
                        return;
                    }

                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error handling link command");
                }
            }
        }
    }
}
