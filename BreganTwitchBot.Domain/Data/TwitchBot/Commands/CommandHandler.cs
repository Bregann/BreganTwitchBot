using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.SuperMods;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands
{
    public class CommandHandler
    {
        public static async Task HandleCommand(OnChatCommandReceivedArgs command)
        {
            //todo: do custom commands

        }

        public static async Task HandleCustomCommand(string? commandName, string username, string userId)
        {
            //Commands beginning with ! are already handled below
            if (commandName == null)
            {
                return;
            }

            if (commandName.StartsWith("!"))
            {
                return;
            }

            using (var context = new DatabaseContext())
            {
                //get the command and check if it exists
                var command = context.Commands.Where(x => x.CommandName == commandName).FirstOrDefault();

                if (command == null)
                {
                    return;
                }

                //Make sure it's not on a cooldown
                if (DateTime.UtcNow - TimeSpan.FromSeconds(5) < command.LastUsed && !Supermods.IsUserSupermod(userId))
                {
                    Log.Information("[Twitch Commands] Custom command handled successfully (cooldown)");
                    return;
                }

                await StreamStatsService.UpdateStreamStat(1, StatTypes.CommandsSent);

                //Send the message
                TwitchHelper.SendMessage(command.CommandText.Replace("[count]", command.TimesUsed.ToString("N0")).Replace("[user]", username));

                //Update the command usage and time
                command.LastUsed = DateTime.UtcNow;
                command.TimesUsed++;
                await context.SaveChangesAsync();

                Log.Information($"[Twitch Commands] Custom command {commandName} handled successfully");
            }
        }
    }
}
