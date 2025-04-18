using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.CustomCommands
{
    public class CustomCommandsDataService(AppDbContext context, ITwitchHelperService twitchHelperService, ICommandHandler commandHandler) : ICustomCommandDataService
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

            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) < commandData.LastUsed && !isSuperMod)
            {
                Log.Information($"Custom command {command} on cooldown for channel {msgParams.BroadcasterChannelName}");
                return;
            }

            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, commandData.CommandText.Replace("[count]", commandData.TimesUsed.ToString()).Replace("[user]", $"@{msgParams.ChatterChannelName}"), msgParams.MessageId);

            if (!isSuperMod)
            {
                commandData.LastUsed = DateTime.UtcNow;
            }

            commandData.TimesUsed++;

            Log.Information($"Custom command {command} used by {msgParams.ChatterChannelName} in channel {msgParams.BroadcasterChannelName}");
            await context.SaveChangesAsync();
        }

        public async Task<string> AddNewCustomCommandAsync(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (msgParams.MessageParts.Length < 3)
            {
                Log.Warning($"User {msgParams.ChatterChannelName} attempted to add a command without the correct format in channel {msgParams.BroadcasterChannelName}");
                throw new InvalidCommandException("You really are a daftylugs! The format is !addcmd commandName commandText");
            }

            var sanitisedCommandName = msgParams.MessageParts[1].ToLower().Trim();

            if (commandHandler.IsSystemCommand(sanitisedCommandName) || await context.CustomCommands.AnyAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == sanitisedCommandName))
            {
                Log.Warning($"User {msgParams.ChatterChannelName} attempted to add a command that already exists in channel {msgParams.BroadcasterChannelName}");
                throw new DuplicateNameException("deary me that command already exists");
            }

            await context.CustomCommands.AddAsync(new CustomCommand
            {
                Channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId),
                CommandName = sanitisedCommandName,
                CommandText = string.Join(" ", msgParams.MessageParts.Skip(2)),
                LastUsed = new DateTime(),
                TimesUsed = 0
            });

            await context.SaveChangesAsync();

            commandHandler.AddCustomCommand(sanitisedCommandName, msgParams.BroadcasterChannelId);

            Log.Information($"User {msgParams.ChatterChannelName} added a new command {sanitisedCommandName} in channel {msgParams.BroadcasterChannelName}");
            return "New command added! Wooooooo";
        }

        public async Task<string> EditCustomCommandAsync(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (msgParams.MessageParts.Length < 3)
            {
                Log.Warning($"User {msgParams.ChatterChannelName} attempted to edit a command without the correct format in channel {msgParams.BroadcasterChannelName}");
                throw new InvalidCommandException("Oh dear, oh dear, oh dear! The format is !editcmd commandName commandText");
            }

            var sanitisedCommandName = msgParams.MessageParts[1].ToLower().Trim();
            var commandContent = string.Join(" ", msgParams.MessageParts.Skip(2));

            var rowsEdited = await context.CustomCommands
                .Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == sanitisedCommandName)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.CommandText, commandContent)
                );

            if (rowsEdited == 0)
            {
                Log.Warning($"User {msgParams.ChatterChannelName} attempted to edit a command that doesn't exist in channel {msgParams.BroadcasterChannelName}");
                throw new CommandNotFoundException("You can't edit a command that doesn't exist");
            }

            Log.Information($"User {msgParams.ChatterChannelName} edited a command {sanitisedCommandName} in channel {msgParams.BroadcasterChannelName}. The new command content is {commandContent}");
            return "Your command has been edited successfully! Why not give it a whirl?";
        }

        public async Task<string> DeleteCustomCommandAsync(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (msgParams.MessageParts.Length < 2)
            {
                Log.Warning($"User {msgParams.ChatterChannelName} attempted to delete a command without the correct format in channel {msgParams.BroadcasterChannelName}");
                throw new InvalidCommandException("Bog off! The format is !delcmd commandName");
            }

            var sanitisedCommandName = msgParams.MessageParts[1].ToLower().Trim();

            var rowsDeleted = await context.CustomCommands
                .Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == sanitisedCommandName)
                .ExecuteDeleteAsync();

            if (rowsDeleted == 0)
            {
                Log.Warning($"User {msgParams.ChatterChannelName} attempted to delete a command that doesn't exist in channel {msgParams.BroadcasterChannelName}");
                throw new CommandNotFoundException("You can't delete a command that doesn't exist FailFish Try again");
            }

            commandHandler.RemoveCustomCommand(sanitisedCommandName, msgParams.BroadcasterChannelId);
            Log.Information($"User {msgParams.ChatterChannelName} deleted a command {sanitisedCommandName} in channel {msgParams.BroadcasterChannelName}");

            //I misspelt it on purpose, it's a meme from the original version of the bot
            return "Your command has been remove! :) It's gone! It's gone! It's gone!";
        }
    }
}
