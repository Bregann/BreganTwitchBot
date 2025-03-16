using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.CustomCommands
{
    public class CustomCommandsDataService (AppDbContext context, ITwitchHelperService twitchHelperService, CommandHandler commandHandler) : ICustomCommandDataService
    {
        public async Task HandleCustomCommandAsync(string command, ChannelChatMessageReceivedParams msgParams)
        {
            var sanitisedCommandName = command.ToLower().Trim();
            var commandData = await context.CustomCommands.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && (x.CommandName == sanitisedCommandName.TrimStart('!') || x.CommandName == sanitisedCommandName));

            if (commandData == null)
            {
                Log.Information($"Custom command {command} not found for channel {msgParams.BroadcasterChannelName}");
                return;
            }

            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelName, msgParams.ChatterChannelName);

            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) < commandData.LastUsed || !isSuperMod)
            {
                Log.Information($"Custom command {command} on cooldown for channel {msgParams.BroadcasterChannelName}");
                return;
            }

            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, commandData.CommandText.Replace("[count]", commandData.TimesUsed.ToString()).Replace("[user]", $"@{msgParams.ChatterChannelName}"), msgParams.MessageId);

            commandData.LastUsed = DateTime.UtcNow;
            commandData.TimesUsed++;
            await context.SaveChangesAsync();
        }

        public async Task<string> AddNewCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var sanitisedCommandName = msgParams.MessageParts[0].ToLower().Trim();

            if (commandHandler.IsSystemCommand(sanitisedCommandName) || await context.CustomCommands.AnyAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == sanitisedCommandName))
            {
                throw new DuplicateNameException("Command already exists");
            }
        }

        public async Task<string> EditCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
        }

        public async Task<string> DeleteCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
        }
    }
}
