using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Api
{
    public interface IPermissionService
    {
        /// <summary>
        /// Whether this twitch user may do something in this channel. The broadcaster
        /// always may.
        /// </summary>
        Task<bool> HasPermissionAsync(string broadcasterChannelName, string twitchUserId, ChannelPermission permission);

        /// <summary>
        /// Whether this twitch user is the broadcaster of this channel
        /// </summary>
        Task<bool> IsBroadcasterAsync(string broadcasterChannelName, string twitchUserId);

        /// <summary>
        /// Everyone with permissions in a channel, and what they hold
        /// </summary>
        Task<List<ChannelPermissionHolderResponse>?> GetPermissionsAsync(string broadcasterChannelName);

        /// <summary>
        /// Replaces someone's permissions in a channel. Broadcaster only.
        /// </summary>
        Task SetPermissionsAsync(string broadcasterChannelName, string grantedByTwitchUserId, string targetTwitchUsername, List<ChannelPermission> permissions);
    }
}
