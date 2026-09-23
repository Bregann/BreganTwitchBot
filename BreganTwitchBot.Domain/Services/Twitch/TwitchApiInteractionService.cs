using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Serilog;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Api.Helix.Models.Chat;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Api.Helix.Models.Moderation.WarnChatUser.Request;

namespace BreganTwitchBot.Domain.Services.Twitch
{
    /// <summary>
    /// Service to interact with the Twitch API. This makes it easier to run unit tests as you can't mock with Twitchlib.
    /// Only methods that need unit testing will be added here.
    /// </summary>
    public class TwitchApiInteractionService : ITwitchApiInteractionService
    {
        public async Task<GetUsersAsyncResponse?> GetUsers(TwitchAPI apiClient, string twitchUsername)
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

        public async Task<GetChannelFollowersAsyncResponse?> GetChannelFollowers(TwitchAPI apiClient, string broadcasterId, string userId)
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

        public async Task<int> GetChannelFollowerCount(TwitchAPI apiClient, string broadcasterId, string moderatorId)
        {
            // first=1 as only the total is wanted, not the follower list itself
            var res = await apiClient.Helix.Channels.GetChannelFollowersAsync(broadcasterId: broadcasterId, first: 1);
            return res.Total;
        }

        public async Task<int> GetChannelSubscriberCount(TwitchAPI apiClient, string broadcasterId)
        {
            var res = await apiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(broadcasterId, first: 1);
            return res.Total;
        }

        public async Task<GetChannelInformationResponse?> GetChannelInformation(TwitchAPI apiClient, string broadcasterId)
        {
            var res = await apiClient.Helix.Channels.GetChannelInformationAsync(broadcasterId);

            if (res.Data.Length == 0)
            {
                return null;
            }

            return new GetChannelInformationResponse
            {
                Title = res.Data[0].Title,
                GameId = res.Data[0].GameId,
                GameName = res.Data[0].GameName
            };
        }

        public async Task ModifyChannelInformation(TwitchAPI apiClient, string broadcasterId, string title, string gameId)
        {
            var request = new ModifyChannelInformationRequest
            {
                Title = title,
                GameId = gameId
            };

            await apiClient.Helix.Channels.ModifyChannelInformationAsync(broadcasterId, request);
        }

        public async Task<(string Id, string Name)?> GetGameByName(TwitchAPI apiClient, string gameName)
        {
            var res = await apiClient.Helix.Games.GetGamesAsync(gameNames: [gameName]);

            // the old bot only null checked the array and then indexed [0], which threw on an
            // unrecognised game name
            if (res.Data == null || res.Data.Length == 0)
            {
                return null;
            }

            return (res.Data[0].Id, res.Data[0].Name);
        }

        public async Task SendChatMessage(TwitchAPI apiClient, string broadcasterChannelId, string twitchChannelClientId, string message, string? originalMessageId = null)
        {
            await apiClient.Helix.Chat.SendChatMessage(broadcasterChannelId, twitchChannelClientId, message, originalMessageId);
        }

        public async Task<GetChattersResponse> GetChatters(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId)
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

            return new GetChattersResponse
            {
                Chatters = chatters
            };
        }

        public async Task SendAnnouncementMessage(TwitchAPI apiClient, string broadcasterChannelId, string twitchChannelClientId, string message)
        {
            await apiClient.Helix.Chat.SendChatAnnouncementAsync(broadcasterChannelId, twitchChannelClientId, message, AnnouncementColors.Green);
        }

        public async Task ShoutoutChannel(TwitchAPI apiClient, string broadcasterChannelId, string shoutoutChannelId, string moderatorId)
        {
            await apiClient.Helix.Chat.SendShoutoutAsync(broadcasterChannelId, shoutoutChannelId, moderatorId);
        }

        public async Task<string?> CreateClip(TwitchAPI apiClient, string broadcasterId)
        {
            var res = await apiClient.Helix.Clips.CreateClipAsync(broadcasterId);

            if (res?.CreatedClips == null || res.CreatedClips.Length == 0)
            {
                return null;
            }

            return res.CreatedClips[0].Id;
        }

        public async Task WarnUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, string message)
        {
            var warn = new WarnChatUserRequest
            {
                UserId = userId,
                Reason = message
            };

            await apiClient.Helix.Moderation.WarnChatUserAsync(broadcasterChannelId, moderatorId, warn);
        }

        public async Task TimeoutUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, int durationInSeconds, string reason)
        {
            var timeout = new BanUserRequest
            {
                UserId = userId,
                Duration = durationInSeconds,
                Reason = reason
            };

            await apiClient.Helix.Moderation.BanUserAsync(broadcasterChannelId, moderatorId, timeout);
        }

        public async Task BanUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, string reason)
        {
            var ban = new BanUserRequest
            {
                UserId = userId,
                Reason = reason
            };

            await apiClient.Helix.Moderation.BanUserAsync(broadcasterChannelId, moderatorId, ban);
        }

        public async Task<GetStreamsResponse?> GetStreams(TwitchAPI apiClient, string broadcasterId)
        {
            try
            {
                var res = await apiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { broadcasterId });

                if (res != null)
                {
                    return res.Streams.Length == 0
                        ? null
                        : new GetStreamsResponse
                        {
                            GameId = res.Streams[0].GameId,
                            GameName = res.Streams[0].GameName,
                            ViewerCount = res.Streams[0].ViewerCount,
                            StartedAt = res.Streams[0].StartedAt
                        };
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Error getting streams for broadcasterId: {broadcasterId}");
                return null;
            }
        }
    }
}
