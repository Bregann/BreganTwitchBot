using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using TwitchLib.Api;

namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITwitchApiInteractionService
    {
        Task<GetUsersAsyncResponse?> GetUsersAsync(TwitchAPI apiClient, string twitchUsername);
        Task<GetChannelFollowersAsyncResponse?> GetChannelFollowersAsync(TwitchAPI apiClient, string broadcasterId, string userId);
        Task SendChatMessage(TwitchAPI apiClient, string broadcasterChannelId, string twitchChannelClientId, string message, string? originalMessageId = null);
        Task<GetChattersResponse> GetChattersAsync(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId);
        Task SendAnnouncementMessage(TwitchAPI apiClient, string broadcasterChannelId, string twitchChannelClientId, string message);
        Task ShoutoutChannel(TwitchAPI apiClient, string broadcasterChannelId, string shoutoutChannelId, string moderatorId);
        Task WarnUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, string message);
        Task TimeoutUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, int durationInSeconds, string reason);
        Task BanUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, string reason);
        Task<GetStreamsResponse?> GetStreams(TwitchAPI apiClient, string broadcasterId);

        /// <summary>
        /// Gets the total follower count for a channel. Unlike GetChannelFollowersAsync this does
        /// not look up a specific follower, so it stays valid for a channel with no followers.
        /// </summary>
        Task<int> GetChannelFollowerCountAsync(TwitchAPI apiClient, string broadcasterId, string moderatorId);

        /// <summary>
        /// Gets the total subscriber count for a channel. Needs the broadcaster's own token.
        /// </summary>
        Task<int> GetChannelSubscriberCountAsync(TwitchAPI apiClient, string broadcasterId);

        /// <summary>
        /// Gets the channel's current title and game
        /// </summary>
        Task<GetChannelInformationResponse?> GetChannelInformationAsync(TwitchAPI apiClient, string broadcasterId);

        /// <summary>
        /// Updates the channel's title and game. Needs the broadcaster's own token.
        /// </summary>
        Task ModifyChannelInformationAsync(TwitchAPI apiClient, string broadcasterId, string title, string gameId);

        /// <summary>
        /// Looks up a game by its exact name. Returns null if Twitch doesn't recognise it.
        /// </summary>
        Task<(string Id, string Name)?> GetGameByNameAsync(TwitchAPI apiClient, string gameName);
    }
}
