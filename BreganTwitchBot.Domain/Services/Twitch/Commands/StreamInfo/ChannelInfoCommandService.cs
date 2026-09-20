using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.StreamInfo
{
    public class ChannelInfoCommandService(IChannelInfoDataService channelInfoDataService, ITwitchHelperService twitchHelperService)
    {
        // static because the command service is scoped - a new instance exists per command,
        // so an instance field would reset the cooldown every time
        private static readonly CommandCooldownHelper TitleCooldown = new(TimeSpan.FromSeconds(5));
        private static readonly CommandCooldownHelper GameCooldown = new(TimeSpan.FromSeconds(5));

        [TwitchCommand("title")]
        public async Task TitleCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var newTitle = msgParams.GetArgument();

            // "!title" on its own shows the title, "!title <something>" sets it
            if (newTitle == null)
            {
                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                if (!isSuperMod && TitleCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
                {
                    return;
                }

                TitleCooldown.StartCooldown(msgParams.BroadcasterChannelId);

                var getResponse = await channelInfoDataService.GetTitle(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, getResponse, msgParams.MessageId);
                return;
            }

            try
            {
                var setResponse = await channelInfoDataService.SetTitle(msgParams, newTitle);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, setResponse, msgParams.MessageId);
            }
            catch (UnauthorizedAccessException ex)
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
        }

        [TwitchCommand("game", ["category"])]
        public async Task GameCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var newGame = msgParams.GetArgument();

            // "!game" on its own shows the game, "!game <something>" sets it
            if (newGame == null)
            {
                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                if (!isSuperMod && GameCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
                {
                    return;
                }

                GameCooldown.StartCooldown(msgParams.BroadcasterChannelId);

                var getResponse = await channelInfoDataService.GetGame(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, getResponse, msgParams.MessageId);
                return;
            }

            try
            {
                var setResponse = await channelInfoDataService.SetGame(msgParams, newGame);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, setResponse, msgParams.MessageId);
            }
            catch (UnauthorizedAccessException ex)
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
        }
    }
}
