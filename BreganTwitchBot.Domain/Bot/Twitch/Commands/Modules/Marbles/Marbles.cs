﻿using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using Serilog;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Marbles
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

            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.Username == command.Command.ArgumentsAsList[0]).First().MarblesWins++;
                context.SaveChanges();
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The marbol win has been added");
        }
    }
}