using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge
{
    public class FollowAgeDataService(TwitchApiConnection twitchApiConnection) : IFollowAgeDataService
    {
        public async Task<string> GetFollowAgeAsync(ChannelChatMessageReceivedParams msgParams)
        {
            var twitchUsernameToLookup = msgParams.MessageParts.Length > 1
                    ? msgParams.MessageParts[1]
                    : msgParams.ChatterChannelName;

            var twitchUserIdToLookup = msgParams.MessageParts.Length > 1
                    ? null
                    : msgParams.ChatterChannelId;

            return await GetUserFollowTime(twitchUsernameToLookup, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, true, twitchUserIdToLookup);
        }

        /// <summary>
        /// Get the follow time of a user in a channel
        /// </summary>
        /// <param name="twitchUsernameToLookup">Always needed. The twitch username to look up</param>
        /// <param name="twitchUserIdToLookup">Optional. If known then supply the user id</param>
        /// <param name="broadcasterUserId">The broadcaster user id</param>
        /// <param name="broadcasterUsername">The broadcaster username</param>
        /// <param name="isFollowAge">True if you want the follow age, false if you want the follow since date</param>
        /// <returns></returns>
        private async Task<string> GetUserFollowTime(string twitchUsernameToLookup, string broadcasterUserId, string broadcasterUsername, bool isFollowAge, string? twitchUserIdToLookup = null)
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
                        return "That username does not exist!";
                    }

                    twitchUserIdToLookup = getUserIdResponse.Users[0].Id;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[Twitch Commands] Error getting the user id from the Twitch api");
                    return $"Well, that's no good! An error has occurred - please try again shortly.";
                }
            }

            try
            {
                var checkFollowResponse = await apiClient.ApiClient.Helix.Channels.GetChannelFollowersAsync(broadcasterId: broadcasterUserId, userId: twitchUserIdToLookup);

                if (checkFollowResponse.Data.Length == 0)
                {
                    return $"It appears {twitchUsernameToLookup} doesn't follow {broadcasterUsername} :(";
                }

                var followTime = DateTime.Parse(checkFollowResponse.Data[0].FollowedAt);

                if (isFollowAge) 
                {
                    // Calculate follow duration in total minutes
                    var nonHumanisedTime = DateTime.UtcNow - followTime;
                    var totalMinutes = (int)nonHumanisedTime.TotalMinutes;
                    return $"{twitchUsernameToLookup} followed {broadcasterUsername} for {totalMinutes} minutes";
}
                else
                {
                    // Return follow date as a readable timestamp
                    return $"{twitchUsernameToLookup} followed {broadcasterUsername} on {followTime:MMMM dd, yyyy 'at' HH:mm UTC}";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Twitch Commands] Error getting the follow time from the Twitch api");
                return "Oh deary me, there has been an error getting the follow age! Try again shortly. poooooo";
            }
        }
    }
}
