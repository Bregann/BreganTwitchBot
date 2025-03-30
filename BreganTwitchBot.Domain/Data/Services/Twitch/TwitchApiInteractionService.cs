using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Moderation.CheckAutoModStatus;
using System.Linq;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    /// <summary>
    /// Service to interact with the Twitch API. This makes it easier to run unit tests as you can't mock with Twitchlib.
    /// Only methods that need unit testing will be added here.
    /// </summary>
    public class TwitchApiInteractionService : ITwitchApiInteractionService
    {
        public async Task<GetUsersAsyncResponse?> GetUsersAsync(TwitchAPI apiClient, string twitchUsername)
        {
            var res = await apiClient.Helix.Users.GetUsersAsync(logins: new List<string> { twitchUsername });

            if (res.Users.Length == 0)
            {
                return null;
            }

            return new GetUsersAsyncResponse
            {
                Users = res.Users.Select(x => new User
                {
                    Id = x.Id,
                    Login = x.Login,
                    DisplayName = x.DisplayName,
                    CreatedAt = x.CreatedAt
                }).ToList()
            };
        }

        public async Task<GetChannelFollowersAsyncResponse?> GetChannelFollowersAsync(TwitchAPI apiClient, string broadcasterId, string userId)
        {
            var res = await apiClient.Helix.Channels.GetChannelFollowersAsync(broadcasterId: broadcasterId, userId: userId);

            if (res.Data.Length == 0)
            {
                return null;
            }

            return new GetChannelFollowersAsyncResponse
            {
                Followers = res.Data.Select(x => new ChannelFollower
                {
                    UserId = x.UserId,
                    UserLogin = x.UserLogin,
                    UserName = x.UserName,
                    FollowedAt = x.FollowedAt
                }).ToList(),
                Total = res.Total
            };
        }

        public async Task SendChatMessage(TwitchAPI apiClient, string broadcasterChannelId, string twitchChannelClientId, string message, string? originalMessageId = null)
        {
            await apiClient.Helix.Chat.SendChatMessage(broadcasterChannelId, twitchChannelClientId, message, originalMessageId);
        }

        public async Task<GetChattersResult> GetChattersAsync(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId)
        {
            var res = await apiClient.Helix.Chat.GetChattersAsync(broadcasterChannelId, moderatorId, 1000);
            
            var chatters = res.Data.Select(item => new Chatters
            {
                UserId = item.UserId,
                UserName = item.UserName
            }).ToList();

            if (res.Pagination.Cursor != null)
            {
                while (res.Pagination.Cursor != null)
                {
                    res = await apiClient.Helix.Chat.GetChattersAsync(broadcasterChannelId, moderatorId, 1000, res.Pagination.Cursor);
                    chatters.AddRange(res.Data.Select(item => new Chatters
                    {
                        UserId = item.UserId,
                        UserName = item.UserName
                    }).ToList());
                }
            }

            return new GetChattersResult
            {
                Chatters = chatters
            };
        }
    }
}
