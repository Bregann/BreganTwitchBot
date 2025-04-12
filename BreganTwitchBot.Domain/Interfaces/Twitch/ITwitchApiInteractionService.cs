using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;

namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITwitchApiInteractionService
    {
        Task<GetUsersAsyncResponse?> GetUsersAsync(TwitchAPI apiClient, string twitchUsername);
        Task<GetChannelFollowersAsyncResponse?> GetChannelFollowersAsync(TwitchAPI apiClient, string broadcasterId, string userId);
        Task SendChatMessage(TwitchAPI apiClient, string broadcasterChannelId, string twitchChannelClientId, string message, string? originalMessageId = null);
        Task<GetChattersResult> GetChattersAsync(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId);
        Task SendAnnouncementMessage(TwitchAPI apiClient, string broadcasterChannelId, string twitchChannelClientId, string message);
        Task ShoutoutChannel(TwitchAPI apiClient, string broadcasterChannelId, string shoutoutChannelId, string moderatorId);
        Task WarnUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, string message);
        Task TimeoutUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, int durationInSeconds, string reason);
        Task BanUser(TwitchAPI apiClient, string broadcasterChannelId, string moderatorId, string userId, string reason);
    }
}
