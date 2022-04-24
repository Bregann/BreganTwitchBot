using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Linking;
using BreganTwitchBot.Data;
using BreganTwitchBot.Data.Models;
using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Twitch.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.Linking
{
    public class Linking
    {
        public static async Task HandleLinkingCommand(OnChatCommandReceivedArgs command)
        {
            //Check if there is a link pending
            DiscordLinkRequests linkReq;

            using(var context = new DatabaseContext())
            {
                linkReq = context.DiscordLinkRequests.Where(x => x.TwitchName == command.Command.ChatMessage.Username.ToLower()).FirstOrDefault();
            }

            if (linkReq == null)
            {
                return;
            }
            
            //Update fields in Db
            using (var context = new DatabaseContext())
            {
                context.DiscordLinkRequests.Remove(linkReq);
                context.Users.Where(x => x.Username == command.Command.ChatMessage.Username.ToLower()).First().DiscordUserId = linkReq.DiscordID;
                context.SaveChanges();
            }

            await DiscordLinking.AddRolesOnInitialVerification(command.Command.ChatMessage.Username.ToLower());
            await DiscordHelper.SendMessage(788112603967782962, $"Welcome <@{linkReq.DiscordID}>! Make sure to read <#754372188324233258> and unlock any channels you're interested in by heading over <#757595460469784597>!"); //Don't link this line
            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Your Twitch and Discord have been linked!");
            return;
        }
    }
}
