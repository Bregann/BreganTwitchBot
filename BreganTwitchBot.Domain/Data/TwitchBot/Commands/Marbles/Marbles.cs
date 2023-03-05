using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Marbles
{
    public class Marbles
    {
        public static async Task HandleAddMarblesWinCommand(string username, string message, List<string> chatArgumentsAsList)
        {
            if (message.ToLower() == "!addmarbleswin")
            {
                TwitchHelper.SendMessage($"@{username} => The usage for this command is !addmarbleswin <username>");
                Log.Information("[Twitch Commands] !addmarbleswin command handled (no paramters)");
                return;
            }

            using (var context = new DatabaseContext())
            {
                var userId = TwitchHelper.GetUserIdFromUsername(chatArgumentsAsList[0].ToLower());

                if (userId == null)
                {
                    TwitchHelper.SendMessage($"@{username} => That username does not exist :(");
                    return;
                }

                context.Users.Where(x => x.TwitchUserId == userId).First().MarblesWins++;
                await context.SaveChangesAsync();
            }

            TwitchHelper.SendMessage($"@{username} => The marbol win has been added");
        }
    }
}