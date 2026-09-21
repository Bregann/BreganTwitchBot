using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Subathon
{
    public class SubathonCommandService(ISubathonDataService subathonDataService, ITwitchHelperService twitchHelperService)
    {
        // static because the command service is scoped - a new instance exists per command,
        // so an instance field would reset the cooldown every time
        private static readonly CommandCooldownHelper SubathonCooldown = new(TimeSpan.FromSeconds(5));

        [TwitchCommand("subathon")]
        public async Task SubathonCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            if (!isSuperMod && SubathonCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
            {
                return;
            }

            SubathonCooldown.StartCooldown(msgParams.BroadcasterChannelId);

            var response = await subathonDataService.GetSubathonStatus(msgParams);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
        }

        [TwitchCommand("startsubathon")]
        public async Task StartSubathonCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await RunModeratorCommand(msgParams, async p => await subathonDataService.StartSubathon(p));
        }

        [TwitchCommand("stopsubathon", ["endsubathon"])]
        public async Task StopSubathonCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await RunModeratorCommand(msgParams, async p => await subathonDataService.StopSubathon(p));
        }

        [TwitchCommand("addsubathontime", ["addsubtime"])]
        public async Task AddSubathonTimeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await RunModeratorCommand(msgParams, async p => await subathonDataService.AddTimeManually(p));
        }

        /// <summary>
        /// The moderator only commands all answer the same way, so the permission failure and
        /// bad input handling lives here rather than being repeated in each
        /// </summary>
        private async Task RunModeratorCommand(ChannelChatMessageReceivedParams msgParams, Func<ChannelChatMessageReceivedParams, Task<string>> action)
        {
            try
            {
                var response = await action(msgParams);
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
