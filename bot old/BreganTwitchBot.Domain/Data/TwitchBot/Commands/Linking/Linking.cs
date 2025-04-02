using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Linking;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Linking
{
    public class Linking
    {
        public static async Task HandleLinkingCommand(string username)
        {
                await DiscordLinking.AddRolesOnInitialVerification(username.ToLower());
                await DiscordHelper.SendMessage(788112603967782962, $"Welcome <@{linkReq.DiscordID}>! Make sure to read <#754372188324233258> and unlock any channels you're interested in by heading over <#757595460469784597>!"); //Don't like this line
            }

        }
    }
}