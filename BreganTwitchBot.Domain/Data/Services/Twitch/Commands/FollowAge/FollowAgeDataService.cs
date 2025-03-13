using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge
{
    public class FollowAgeDataService(TwitchApiConnection twitchApiConnection) : IFollowAgeDataService
    {
        public async Task<string> HandleFollowCommandAsync(ChannelChatMessageReceivedParams msgParams, FollowCommandTypeEnum followCommandType)
        {
            var twitchUsernameToLookup = msgParams.MessageParts.Length > 1
                    ? msgParams.MessageParts[1]
                    : msgParams.ChatterChannelName;

            var twitchUserIdToLookup = msgParams.MessageParts.Length > 1
                    ? null
                    : msgParams.ChatterChannelId;

            var followTime = await GetUserFollowTime(twitchUsernameToLookup, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, twitchUserIdToLookup);

            switch (followCommandType)
            {
                case FollowCommandTypeEnum.FollowAge:
                    if (followTime.Item1 != null)
                    {
                        var nonHumanisedTime = DateTime.UtcNow - followTime.Item1.Value;

                        var formattedTime = $"{(int)(nonHumanisedTime.TotalDays / 365)} years, " +
                        $"{nonHumanisedTime.Days % 365} days, " +
                        $"{nonHumanisedTime.Hours} hours, " +
                        $"{nonHumanisedTime.Minutes} minutes, " +
                        $"{nonHumanisedTime.Seconds} seconds";

                        return $"{twitchUsernameToLookup} followed {msgParams.BroadcasterChannelName} for {formattedTime} minutes";
                    }
                    break;
                case FollowCommandTypeEnum.FollowSince:
                    if (followTime.Item1 != null)
                    {
                        return $"{twitchUsernameToLookup} followed {msgParams.BroadcasterChannelName} on {followTime:MMMM dd, yyyy 'at' HH:mm}";
                    }
                    break;
                case FollowCommandTypeEnum.FollowMinutes:
                    if (followTime.Item1 != null)
                    {
                        var nonHumanisedTime = DateTime.UtcNow - followTime.Item1.Value;
                        var totalMinutes = (int)nonHumanisedTime.TotalMinutes;
                        return $"{twitchUsernameToLookup} followed {msgParams.BroadcasterChannelName} for {totalMinutes} minutes";
                    }
                    break;
                default:
                    return "idk how you got here but you somehow did";
            }

            return followTime.Item2;
        }

        /// <summary>
        /// Get the follow time of a user in a channel
        /// </summary>
        /// <param name="twitchUsernameToLookup">Always needed. The twitch username to look up</param>
        /// <param name="twitchUserIdToLookup">Optional. If known then supply the user id</param>
        /// <param name="broadcasterUserId">The broadcaster user id</param>
        /// <param name="broadcasterUsername">The broadcaster username</param>
        /// <returns></returns>
        private async Task<(DateTime?, string)> GetUserFollowTime(string twitchUsernameToLookup, string broadcasterUserId, string broadcasterUsername, string? twitchUserIdToLookup = null)
        {
            var apiClient = twitchApiConnection.GetTwitchApiClientFromChannelName(broadcasterUsername);

            if (apiClient == null)
            {
                throw new ArgumentException("Could not get the twitch api client from the broadcaster username");
            }

            if (twitchUserIdToLookup == null)
            {
                try
                {
                    var getUserIdResponse = await apiClient.ApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { twitchUsernameToLookup });

                    if (getUserIdResponse.Users.Length == 0)
                    {
                        return (null, "That username does not exist!");
                    }

                    twitchUserIdToLookup = getUserIdResponse.Users[0].Id;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[Twitch Commands] Error getting the user id from the Twitch api");
                    return (null, $"Well, that's no good! An error has occurred - please try again shortly.");
                }
            }

            try
            {
                var checkFollowResponse = await apiClient.ApiClient.Helix.Channels.GetChannelFollowersAsync(broadcasterId: broadcasterUserId, userId: twitchUserIdToLookup);

                if (checkFollowResponse.Data.Length == 0)
                {
                    return (null, $"It appears {twitchUsernameToLookup} doesn't follow {broadcasterUsername} :(");
                }

                var followTime = DateTime.Parse(checkFollowResponse.Data[0].FollowedAt);

                return (followTime, "");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Twitch Commands] Error getting the follow time from the Twitch api");
                return (null, "Oh deary me, there has been an error getting the follow age! Try again shortly. poooooo");
            }
        }
    }
}
