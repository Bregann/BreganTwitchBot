using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.FollowAge
{
    public class FollowAge
    {
        private static DateTime _followAgeCooldown;
        private static DateTime _followSinceCooldown;

        public static async Task HandleFollowageCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _followAgeCooldown && !TwitchHelper.IsUserSupermod(userId))
            {
                Log.Information("[Twitch Commands] !followage command handled successfully (cooldown)");
                return;
            }

            if (message.ToLower() == "!followage" || message.ToLower() == "!howlong")
            {
                TwitchHelper.SendMessage($"@{username} => {await GetUserFollowTime(username.ToLower(), true)}!");
                _followAgeCooldown = DateTime.UtcNow;
                return;
            }

            TwitchHelper.SendMessage($"@{username} => {await GetUserFollowTime(chatArgumentsAsList[0].ToLower().Replace("@", ""), true)}!");
            _followAgeCooldown = DateTime.UtcNow;
        }

        public static async Task HandleFollowSinceCommand(string username, string userId, string message, List<string> chatArgumentsAsList)
        {
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _followSinceCooldown)
            {
                Log.Information("[Twitch Commands] !followsince command handled successfully (cooldown)");
                return;
            }

            if (message.ToLower() == "!followsince")
            {
                TwitchHelper.SendMessage($"@{username} => You followed {AppConfig.BroadcasterName} at {await GetUserFollowTime(username.ToLower(), false)}!");
                _followSinceCooldown = DateTime.UtcNow;
                return;
            }

            TwitchHelper.SendMessage($"@{username} => {await GetUserFollowTime(chatArgumentsAsList[0].ToLower().Replace("@", ""), false)}!");
            _followSinceCooldown = DateTime.UtcNow;
        }

        private static async Task<string> GetUserFollowTime(string username, bool isFollowAge)
        {
            GetUsersResponse getUserID;
            try
            {
                getUserID = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { username });
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting user - {e}");
                return "Well thats no good! An error has occured - please try again shortly";
            }

            //check if the username exists
            if (getUserID.Users.Length == 0)
            {
                return $"That username does not exist!";
            }

            GetUsersFollowsResponse checkFollow;
            try
            {
                checkFollow = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(fromId: getUserID.Users[0].Id, toId: AppConfig.TwitchChannelID);
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting user - {e}");
                return "Oh deary me there has been an error getting the follow age! Try again shortly";
            }

            //Check if they are followed as i forgot to do this and I looked silly
            if (checkFollow.Follows.Length == 0)
            {
                return $"It appears {username} doesn't follow {AppConfig.BroadcasterName} :(";
            }

            //To save repeating code I just added a bool to return either followsince or followage
            if (isFollowAge)
            {
                //now we get the follow age and make it pwetty witty :3
                var followTime = checkFollow.Follows[0].FollowedAt;
                var nonHumanisedTime = DateTime.UtcNow - followTime;
                return $"{username} followed {AppConfig.BroadcasterName} since {nonHumanisedTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}";
            }
            else
            {
                return $"{username} followed {AppConfig.BroadcasterName} at {checkFollow.Follows[0].FollowedAt}";
            }
        }
    }
}