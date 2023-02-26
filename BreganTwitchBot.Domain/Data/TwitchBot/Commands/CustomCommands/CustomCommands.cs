using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.CustomCommands
{
    public class CustomCommands
    {
        public static async Task HandleAddCommand(string username, List<string> commandNameList, string commandText)
        {
            //See if they've done it right or not
            if (commandNameList.Count < 2)
            {
                TwitchHelper.SendMessage($"@{username} => The usage for this command is !addcmd <command name> <command text> :)");
                Log.Information("[Twitch Commands] !addcmd handled successfully (gave advice)");
                return;
            }

            using (var context = new DatabaseContext())
            {
                var cmd = context.Commands.Where(x => x.CommandName == commandNameList[0].ToLower()).FirstOrDefault();

                //Make sure it doesn't exist
                if (cmd != null)
                {
                    TwitchHelper.SendMessage($"@{username} => You donut! That command already exists KEKW");
                    Log.Information("[Twitch Commands] !addcmd handled successfully (invalid/already exists)");
                    return;
                }
            
                //Create the command text and name
                var commandName = commandNameList[0].ToLower();
                commandText = commandText.Remove(0, commandName.Length + 1);

                context.Commands.Add(new Infrastructure.Database.Models.Commands
                {
                    CommandName = commandName,
                    CommandText = commandText,
                    TimesUsed = 0,
                    LastUsed = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            TwitchHelper.SendMessage($"@{username} => Your command has been added! :)");
            return;
        }

        public static async Task HandleRemoveCommand(string username, string commandName)
        {
            //check if command has parameters
            if (commandName == "!delcmd" || commandName == "!cmddel")
            {
                TwitchHelper.SendMessage($"@{username} => The usage for this command is !delcmd <command name> :)");
                Log.Information("[Twitch Commands] !delcmd handled successfully (gave advice)");
                return;
            }

            using (var context = new DatabaseContext())
            {
                var commandAmountDeleted = await context.Commands.Where(x => x.CommandName == commandName).ExecuteDeleteAsync();

                if (commandAmountDeleted == 0)
                {
                    TwitchHelper.SendMessage($"@{username} => 404 command not found");
                    Log.Information("[Twitch Commands] !delcmd handled successfully (command doesn't exist)");
                    return;
                }

                await context.SaveChangesAsync();
            }

            TwitchHelper.SendMessage($"@{username} => Your command has been remove! :)");
        }

        public static async Task HandleEditCommandCommand(string username, List<string> commandNameList, string commandText)
        {
            //check if command has parameters
            if (commandNameList.Count < 2)
            {
                TwitchHelper.SendMessage($"@{username} => The usage for this command is !editcmd <command name> <command text> :)");
                Log.Information("[Twitch Commands] !editcmd handled successfully (gave advice)");
                return;
            }

            using (var context = new DatabaseContext())
            {
                var commandName = commandNameList[0].ToLower();
                var commandFromDb = context.Commands.Where(x => x.CommandName == commandName).FirstOrDefault();


                if (commandFromDb == null)
                {
                    TwitchHelper.SendMessage($"@{username} => One cannot edit a command that does not exist");
                    Log.Information("[Twitch Commands] !editcmd handled successfully (command doesn't exist)");
                    return;
                }

                //update it
                commandText = commandText.Remove(0, commandName.Length + 1);

                commandFromDb.CommandName = commandName;
                commandFromDb.CommandText = commandText;

                await context.SaveChangesAsync();
            }

            CommandHandler.UpdateCustomCommandsList();
            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Your command has been edited! :)");
        }
    }
}