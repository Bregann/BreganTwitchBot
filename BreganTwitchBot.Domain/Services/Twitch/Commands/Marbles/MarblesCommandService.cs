using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Marbles
{
    public class MarblesCommandService(IMarblesDataService marblesDataService, ITwitchHelperService twitchHelperService)
    {
        [TwitchCommand("addmarbleswin", ["addmarblewin", "addmarbleswins"])]
        public async Task AddMarblesWinCommand(ChannelChatMessageReceivedParams msgParams)
        {
            try
            {
                var response = await marblesDataService.AddMarblesWin(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
            catch (Exception ex)
            when (
                ex is UnauthorizedAccessException ||
                ex is InvalidCommandException ||
                ex is TwitchUserNotFoundException
            )
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
        }

        [TwitchCommand("marbles", ["marbleswins", "marblewins"])]
        public async Task GetMarblesWinsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            try
            {
                var response = await marblesDataService.GetMarblesWins(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
            catch (TwitchUserNotFoundException ex)
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
        }
    }
}
