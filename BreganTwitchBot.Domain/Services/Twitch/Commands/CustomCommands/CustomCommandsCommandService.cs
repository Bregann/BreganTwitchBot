using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using System.Data;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.CustomCommands
{
    public class CustomCommandsCommandService(ICustomCommandDataService customCommandsDataService, ITwitchHelperService twitchHelperService)
    {
        [TwitchCommand("addcmd", ["addcommand", "cmdadd", "commandadd"])]
        public async Task AddCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
            try
            {
                var response = await customCommandsDataService.AddNewCustomCommandAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
            catch (Exception ex)
            when (
                ex is UnauthorizedAccessException ||
                ex is InvalidCommandException ||
                ex is DuplicateNameException
            )
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
        }

        [TwitchCommand("editcmd", ["editcommand", "cmdedit", "commandedit"])]
        public async Task EditCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
            try
            {
                var response = await customCommandsDataService.EditCustomCommandAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
            catch (Exception ex)
            when (
                ex is UnauthorizedAccessException ||
                ex is CommandNotFoundException ||
                ex is InvalidCommandException
            )
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
        }

        [TwitchCommand("delcmd", ["delcommand", "cmddel", "commanddel"])]
        public async Task DeleteCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
            try
            {
                var response = await customCommandsDataService.DeleteCustomCommandAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
            catch (Exception ex)
            when (
                ex is UnauthorizedAccessException ||
                ex is CommandNotFoundException ||
                ex is InvalidCommandException
            )
            {
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
            }
        }
    }
}
