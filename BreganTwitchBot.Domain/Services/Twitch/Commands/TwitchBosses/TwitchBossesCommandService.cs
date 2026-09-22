using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.TwitchBosses
{
    public class TwitchBossesCommandService(ITwitchBossesDataService twitchBossesDataService, ITwitchHelperService twitchHelperService)
    {
        [TwitchCommand("boss")]
        public async Task HandleBossCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var bossResponse = twitchBossesDataService.HandleBossCommand(msgParams);

            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, bossResponse, msgParams.MessageId);
        }

        [TwitchCommand("startboss")]
        public async Task HandleStartBossCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchBossesDataService.StartBossFightCountdown(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, msgParams);
        }
    }
}
