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

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.TwitchBosses
{
    public class TwitchBossesCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("boss")]
        public void HandleBossCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchBossesDataService = scope.ServiceProvider.GetRequiredService<ITwitchBossesDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                var bossResponse = twitchBossesDataService.HandleBossCommand(msgParams);

                twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, bossResponse, msgParams.MessageId);
            }
        }

        [TwitchCommand("startboss")]
        public async Task HandleStartBossCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchBossesDataService = scope.ServiceProvider.GetRequiredService<ITwitchBossesDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                await twitchBossesDataService.StartBossFightCountdown(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, msgParams);
            }
        }
    }
}
