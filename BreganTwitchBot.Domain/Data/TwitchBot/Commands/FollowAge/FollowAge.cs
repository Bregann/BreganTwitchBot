using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
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
                return "Well, that's no good! An error has occurred - please try again shortly.";
            }

            // Check if the username exists
            if (getUserID.Users.Length == 0)
            {
                return $"That username does not exist!";
            }

            // Get the user's ID
            string userId = getUserID.Users[0].Id;

            GetChannelFollowersResponse checkFollow;
            try
            {
                checkFollow = await TwitchApiConnection.ApiClient.Helix.Channels.GetChannelFollowersAsync(broadcasterId: AppConfig.TwitchChannelID, userId: userId, accessToken: AppConfig.BroadcasterOAuth);
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting user follow - {e}");
                return "Oh deary me, there has been an error getting the follow age! Try again shortly. poooooo";
            }

            // Check if they follow the channel
            if (checkFollow.Data.Length == 0)
            {
                return $"It appears {username} doesn't follow {AppConfig.BroadcasterName} :(";
            }

            var followData = checkFollow.Data.FirstOrDefault();
            if (followData == null)
            {
                return $"It appears {username} doesn't follow {AppConfig.BroadcasterName} :(";
            }

            // Convert the FollowedAt string to a DateTime (Twitch returns ISO 8601 strings)
            DateTime followTime;
            try
            {
                followTime = DateTime.Parse(followData.FollowedAt);
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error parsing FollowedAt date - {e}");
                return "Oh dear, we had trouble reading the follow time. Please try again shortly.";
            }

            if (isFollowAge)
            {
                // Calculate follow duration and humanize it
                var nonHumanisedTime = DateTime.UtcNow - followTime;
                return $"{username} followed {AppConfig.BroadcasterName} for {nonHumanisedTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}";
            }
            else
            {
                // Return follow date as a readable timestamp
                return $"{username} followed {AppConfig.BroadcasterName} on {followTime:MMMM dd, yyyy 'at' HH:mm UTC}";
            }
        }
    }
}