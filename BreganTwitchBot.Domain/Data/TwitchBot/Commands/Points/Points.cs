using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Points
{
    public class Points
    {
        public static void HandlePointsCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (message.ToLower() == "!points" ||
                message.ToLower() == "!coins" ||
                message.ToLower() == "!pooants")
            {
                TwitchHelper.SendMessage($"@{username} => You have {PointsHelper.GetUserPoints(userId):N0} {AppConfig.PointsName}. Rank: {GetPointsRank(userId)}");
                return;
            }

            var otherUserId = TwitchHelper.GetUserIdFromUsername(chatArgumentsAsList[0].Replace("@", "").ToLower());

            if (otherUserId == null)
            {
                TwitchHelper.SendMessage($"@{username} => That user does not exist :(");
                return;
            }

            TwitchHelper.SendMessage($"@{username} => {chatArgumentsAsList[0].Replace("@", "")} has {PointsHelper.GetUserPoints(otherUserId):N0} {AppConfig.PointsName} Rank: {GetPointsRank(otherUserId)}");
        }

        public static async Task HandleAddPointsCommand(string username, List<string> chatArgumentsAsList)
        {
            if (chatArgumentsAsList.Count < 2)
            {
                TwitchHelper.SendMessage($"@{username} => The format for adding {AppConfig.PointsName} is !addpoints <username> <points>");
                Log.Information("[Twitch Commands] !addpoints command handled successfully (no parameters)");
                return;
            }

            long.TryParse(chatArgumentsAsList[1], out var pointsResult);

            if (pointsResult < 0 || pointsResult == 0)
            {
                TwitchHelper.SendMessage($"@{username} => lol no (usage for this command is !addpoints <username> <points>)");
                Log.Information("[Twitch Commands] !addpoints command handled successfully (bad bot break attempt)");
                return;
            }

            var userId = TwitchHelper.GetUserIdFromUsername(chatArgumentsAsList[0].Replace("@", "").ToLower());

            if (userId == null)
            {
                TwitchHelper.SendMessage($"@{username} => That user does not exist :(");
                return;
            }

            await PointsHelper.AddUserPoints(userId, pointsResult);
            TwitchHelper.SendMessage($"@{username} => added");
        }

        private static string GetPointsRank(string userId)
        {
            using (var context = new DatabaseContext())
            {
                var position = context.Users.Where(x => x.Points > 0).OrderByDescending(x => x.Points).Select(x => x.TwitchUserId).ToList();
                return $"{position.IndexOf(userId) + 1} / {position.Count}";
            }
        }
    }
}