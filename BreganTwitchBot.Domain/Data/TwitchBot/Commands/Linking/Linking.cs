using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.Linking;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Linking
{
    public class Linking
    {
        public static async Task HandleLinkingCommand(string username)
        {
            using (var context = new DatabaseContext())
            {
                //Check if theres a link pending

                var linkReq = context.DiscordLinkRequests.Where(x => x.TwitchName == username.ToLower()).FirstOrDefault();

                if (linkReq == null)
                {
                    return;
                }

                //Update fields in Db
                context.DiscordLinkRequests.Remove(linkReq);
                context.Users.Where(x => x.Username == username.ToLower()).First().DiscordUserId = linkReq.DiscordID;
                context.SaveChanges();

                await DiscordLinking.AddRolesOnInitialVerification(username.ToLower());
                await DiscordHelper.SendMessage(788112603967782962, $"Welcome <@{linkReq.DiscordID}>! Make sure to read <#754372188324233258> and unlock any channels you're interested in by heading over <#757595460469784597>!"); //Don't like this line
                TwitchHelper.SendMessage($"@{username} => Your Twitch and Discord have been linked!");
            }

        }
    }
}