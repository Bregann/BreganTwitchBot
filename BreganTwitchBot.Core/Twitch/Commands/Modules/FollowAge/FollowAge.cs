using BreganTwitchBot.Services;
using BreganTwitchBot.Twitch.Helpers;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.FollowAge
{
    public class FollowAge
    {
        private static DateTime _followAgeCooldown;
        private static DateTime _followSinceCooldown;

        public static async Task HandleFollowageCommand(OnChatCommandReceivedArgs command)
        {
            if (DateTime.Now - TimeSpan.FromSeconds(5) <= _followAgeCooldown && !SuperMods.SuperMods.IsUserSupermod(command.Command.ChatMessage.Username.ToLower()))
            {
                Log.Information("[Twitch Commands] !followage command handled successfully (cooldown)");
                return;
            }

            if (command.Command.ChatMessage.Message.ToLower() == "!followage" || command.Command.ChatMessage.Message.ToLower() == "!howlong")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {await GetUserFollowTime(command.Command.ChatMessage.Username.ToLower(), true)}!");
                _followAgeCooldown = DateTime.Now;
                return;
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {await GetUserFollowTime(command.Command.ArgumentsAsList[0].ToLower(), true)}!");
            _followAgeCooldown = DateTime.Now;
        }

        public static async Task HandleFollowSinceCommand(OnChatCommandReceivedArgs command)
        {
            if (DateTime.Now - TimeSpan.FromSeconds(5) <= _followSinceCooldown)
            {
                Log.Information("[Twitch Commands] !followsince command handled successfully (cooldown)");
                return;
            }

            if (command.Command.ChatMessage.Message.ToLower() == "!followsince")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You followed {Config.BroadcasterName} at {await GetUserFollowTime(command.Command.ChatMessage.Username.ToLower(), false)}!");
                _followSinceCooldown = DateTime.Now;
                return;
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => {await GetUserFollowTime(command.Command.ArgumentsAsList[0].ToLower(), false)}!");
            _followSinceCooldown = DateTime.Now;
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
                checkFollow = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(fromId: getUserID.Users[0].Id, toId: Config.TwitchChannelID);

            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting user - {e}");
                return "Oh deary me there has been an error getting the follow age! Try again shortly";
            }

            //Check if they are followed as i forgot to do this and I looked silly
            if (checkFollow.Follows.Length == 0)
            {
                return $"It appears {username} doesn't follow {Config.BroadcasterName} :(";
            }

            //To save repeating code I just added a bool to return either followsince or followage
            if (isFollowAge)
            {
                //now we get the follow age and make it pwetty witty :3
                var followTime = checkFollow.Follows[0].FollowedAt;
                var nonHumanisedTime = DateTime.Now - followTime;
                return $"{username} followed {Config.BroadcasterName} since {nonHumanisedTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}";
            }
            else
            {
                return $"{username} followed {Config.BroadcasterName} at {checkFollow.Follows[0].FollowedAt}";
            }
        }
    }
}
