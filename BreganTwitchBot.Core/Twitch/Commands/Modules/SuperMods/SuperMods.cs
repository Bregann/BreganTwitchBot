using BreganTwitchBot.Data;
using BreganTwitchBot.Twitch.Helpers;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.SuperMods
{
    public class SuperMods
    {
        private static List<string> _superModIds;

        public static bool IsUserSupermod(string userId)
        {
            //If the supermods haven't been loaded
            if (_superModIds == null)
            {
                using (var context = new DatabaseContext())
                {
                    _superModIds = context.Users.Where(x => x.IsSuperMod == true).Select(x => x.TwitchUserId).ToList();
                }
            }

            return _superModIds.Contains(userId);
        }

        public static async Task HandleAddSupermodCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ArgumentsAsList.Count == 0)
            {
                return;
            }

            //Check if they're already a sub
            using (var context = new DatabaseContext())
            {
                if (context.Users.Where(x => x.Username == command.Command.ArgumentsAsList[0].ToLower() && x.IsSuperMod == true).Any())
                {
                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].ToLower()} is already a supermod!");
                    return;
                }
                else
                {
                    //Update the user and save it
                    context.Users.Where(x => x.Username == command.Command.ArgumentsAsList[0].ToLower()).First().IsSuperMod = true;
                    await context.SaveChangesAsync();
                }
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].ToLower()} has been added as a supermod!");
        }
        public static async Task HandleRemoveSupermodCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ArgumentsAsList.Count == 0)
            {
                return;
            }

            //Check if they're already a sub
            using (var context = new DatabaseContext())
            {
                if (context.Users.Where(x => x.Username == command.Command.ArgumentsAsList[0].ToLower() && x.IsSuperMod == false).Any())
                {
                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].ToLower()} is not a supermod!");
                    return;
                }
                else
                {
                    //Update the user and save it
                    context.Users.Where(x => x.Username == command.Command.ArgumentsAsList[0].ToLower()).First().IsSuperMod = false;
                    await context.SaveChangesAsync();
                }
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].ToLower()} has been removed as a supermod!");
        }
    }
}
