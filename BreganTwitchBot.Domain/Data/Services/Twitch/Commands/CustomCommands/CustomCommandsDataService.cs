using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
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

            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

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
            if(msgParams.MessageParts.Length < 3)
            {
                throw new InvalidCommandException("You really are a daftylugs! The format is !addcmd commandName commandText");
            }

            var sanitisedCommandName = msgParams.MessageParts[1].ToLower().Trim();

            if (commandHandler.IsSystemCommand(sanitisedCommandName) || await context.CustomCommands.AnyAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == sanitisedCommandName))
            {
                throw new DuplicateNameException("deary me that command already exists");
            }

            await context.CustomCommands.AddAsync(new CustomCommand
            {
                Channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId),
                CommandName = sanitisedCommandName,
                CommandText = string.Join(" ", msgParams.MessageParts.Skip(2)),
                LastUsed = DateTime.UtcNow.AddSeconds(-10),
                TimesUsed = 0
            });

            await context.SaveChangesAsync();
            return "New command added! Wooooooo";
        }

        public async Task<string> EditCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
            return "";
        }

        public async Task<string> DeleteCustomCommand(ChannelChatMessageReceivedParams msgParams)
        {
            return "";
        }
    }
}
