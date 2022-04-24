using BreganTwitchBot.Data;
using BreganTwitchBot.Services;
using BreganTwitchBot.Twitch.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.Marbles
{
    public class Marbles
    {
        public static async Task HandleAddMarblesWinCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message.ToLower() == "!addmarbleswin")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The usage for this command is !addmarbleswin <username>");
                Log.Information("[Twitch Commands] !addmarbleswin command handled (no paramters)");
                return;
            }

            using(var context = new DatabaseContext())
            {
                context.Users.Where(x => x.Username == command.Command.ArgumentsAsList[0]).First().MarblesWins++;
                context.SaveChanges();
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The marbol win has been added");
        }
    }
}
