using BreganTwitchBot.Core.Twitch.Helpers;
using BreganTwitchBot.Data;
using BreganTwitchBot.Twitch.Helpers;
using Serilog;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.Points
{
    public class Points
    {
        public static void HandlePointsCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message.ToLower() == "!points" || command.Command.ChatMessage.Message.ToLower() == "!coins" || command.Command.ChatMessage.Message.ToLower() == "!pooants")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You have {PointsHelper.GetUserPoints(command.Command.ChatMessage.Username):N0} {Config.PointsName}. Rank: {GetPointsRank(command.Command.ChatMessage.Username)}");
                return;
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {command.Command.ArgumentsAsList[0].Replace("@", "")} has {PointsHelper.GetUserPoints(command.Command.ArgumentsAsList[0].ToLower().Replace("@", "")):N0} {Config.PointsName} Rank: {GetPointsRank(command.Command.ArgumentsAsList[0].ToLower())}");
        }

        public static void HandleAddPointsCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ArgumentsAsList.Count < 2)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The format for adding {Config.PointsName} is !addpoints <username> <points>");
                Log.Information("[Twitch Commands] !addpoints command handled successfully (no parameters)");
                return;
            }

            long.TryParse(command.Command.ArgumentsAsList[1], out var pointsResult);

            if (pointsResult < 0 || pointsResult == 0)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => lol no (usage for this command is !addpoints <username> <points>)");
                Log.Information("[Twitch Commands] !addpoints command handled successfully (bad bot break attempt)");
                return;
            }

            PointsHelper.AddUserPoints(command.Command.ArgumentsAsList[0].ToLower(), pointsResult);
            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => added");
        }

        private static string GetPointsRank(string username)
        {
            using (var context = new DatabaseContext())
            {
                var position = context.Users.OrderByDescending(x => x.Points).Select(x => x.Username).ToList();
                return $"{position.IndexOf(username) + 1} / {position.Count}";
            }
        }
    }
}
