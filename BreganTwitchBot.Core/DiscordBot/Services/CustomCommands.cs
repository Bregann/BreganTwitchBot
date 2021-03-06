using BreganTwitchBot.Data;
using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Services;
using Discord.WebSocket;
using Serilog;

namespace BreganTwitchBot.Core.DiscordBot.Services
{
    public class CustomCommands
    {
        private static Dictionary<string, string> _commands;

        public static void SetupCustomCommands()
        {
            DiscordConnection.DiscordClient.MessageReceived += DiscordClient_MessageReceived;

            using (var context = new DatabaseContext())
            {
                _commands = context.Commands.ToDictionary(x => x.CommandName, x => x.CommandText);
            }

            Log.Information($"[Discord Custom Commands] {_commands.Count} commands loaded from database");
        }

        private static async Task DiscordClient_MessageReceived(SocketMessage arg)
        {
            var isMod = DiscordHelper.IsUserMod(arg.Author.Id);
            var commandName = arg.Content.Split(" ")[0].ToLower();

            //Check if its actually a command
            if (!commandName.StartsWith('!') || arg.Author.IsBot)
            {
                return;
            }

            if (arg.Channel.Id != Config.DiscordCommandsChannelID && !isMod)
            {
                return;
            }

            if (!_commands.ContainsKey(commandName))
            {
                return;
            }

            var commandText = _commands[commandName];

            //todo: make it so you can use user & count
            if (commandText.Contains("[user]") || commandText.Contains("[count]"))
            {
                return;
            }

            //check if there are quotes
            if (commandText.Contains("''"))
            {
                commandText = commandText.Replace("''", "'");
            }

            await arg.Channel.TriggerTypingAsync();
            await arg.Channel.SendMessageAsync(commandText);
        }
    }
}
