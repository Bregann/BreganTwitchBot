using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.CustomCommands
{
    public class CustomCommands
    {
        public static void HandleAddCommand(OnChatCommandReceivedArgs command)
        {
            //See if they've done it right or not
            if (command.Command.ArgumentsAsList.Count < 2)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The usage for this command is !addcmd <command name> <command text> :)");
                Log.Information("[Twitch Commands] !addcmd handled successfully (gave advice)");
                return;
            }

            var commandExists = false;

            using (var context = new DatabaseContext())
            {
                var cmd = context.Commands.Where(x => x.CommandName == command.Command.ArgumentsAsList[0].ToLower()).FirstOrDefault();

                if (cmd != null)
                {
                    commandExists = true;
                }
            }

            //Make sure it doesn't exist
            if (commandExists)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You donut! That command already exists KEKW");
                Log.Information("[Twitch Commands] !addcmd handled successfully (invalid/already exists)");
                return;
            }

            //Create the command text and name
            var commandName = command.Command.ArgumentsAsList[0].ToLower();
            var commandText = command.Command.ArgumentsAsString.Remove(0, commandName.Length + 1);

            using (var context = new DatabaseContext())
            {
                context.Commands.Add(new Infrastructure.Database.Models.Commands
                {
                    CommandName = commandName,
                    CommandText = commandText
                });

                context.SaveChanges();
            }

            CommandHandler.UpdateCustomCommandsList();
            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Your command has been added! :)");
            return;
        }

        public static void HandleRemoveCommand(OnChatCommandReceivedArgs command)
        {
            //check if command has parameters
            if (command.Command.ChatMessage.Message == "!delcmd" || command.Command.ChatMessage.Message == "!cmddel")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The usage for this command is !delcmd <command name> :)");
                Log.Information("[Twitch Commands] !delcmd handled successfully (gave advice)");
                return;
            }

            //does it exist
            var commandExists = false;
            var cmdText = "";

            using (var context = new DatabaseContext())
            {
                var cmd = context.Commands.Where(x => x.CommandName == command.Command.ArgumentsAsList[0].ToLower()).FirstOrDefault();

                if (cmd != null)
                {
                    commandExists = true;
                    cmdText = cmd.CommandText;
                }
            }

            if (!commandExists)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => 404 command not found");
                Log.Information("[Twitch Commands] !delcmd handled successfully (command doesn't exist)");
                return;
            }

            //hoo it does lets remove it
            var commandName = command.Command.ArgumentsAsList[0].ToLower();

            using (var context = new DatabaseContext())
            {
                context.Commands.Remove(new Infrastructure.Database.Models.Commands
                {
                    CommandName = commandName,
                    CommandText = cmdText
                });

                context.SaveChanges();
            }

            CommandHandler.UpdateCustomCommandsList();
            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Your command has been remove! :)");
        }

        public static void HandleEditCommandCommand(OnChatCommandReceivedArgs command)
        {
            //check if command has parameters
            if (command.Command.ArgumentsAsList.Count < 2)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The usage for this command is !editcmd <command name> <command text> :)");
                Log.Information("[Twitch Commands] !editcmd handled successfully (gave advice)");
                return;
            }

            //Check if the command exists
            var commandExists = false;

            using (var context = new DatabaseContext())
            {
                if (context.Commands.Where(x => x.CommandName == command.Command.ArgumentsAsList[0].ToLower()).Any())
                {
                    commandExists = true;
                }
            }

            if (!commandExists)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => One cannot edit a command that does not exist");
                Log.Information("[Twitch Commands] !editcmd handled successfully (command doesn't exist)");
                return;
            }

            //update it
            var commandName = command.Command.ArgumentsAsList[0].ToLower();
            var commandText = command.Command.ArgumentsAsString.Remove(0, commandName.Length + 1);

            using (var context = new DatabaseContext())
            {
                context.Commands.Update(new Infrastructure.Database.Models.Commands
                {
                    CommandName = commandName,
                    CommandText = commandText
                });

                context.SaveChanges();
            }

            CommandHandler.UpdateCustomCommandsList();
            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Your command has been edited! :)");
        }
    }
}