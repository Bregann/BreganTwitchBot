using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.DTOs.Api
{
    public class ChannelPermissionHolderResponse
    {
        public required string TwitchUsername { get; set; }
        public required string TwitchUserId { get; set; }
        public required List<ChannelPermission> Permissions { get; set; }
        public required DateTime GrantedAt { get; set; }
        public required string GrantedByTwitchUserId { get; set; }
    }

    public class SetPermissionsRequest
    {
        public required string TwitchUsername { get; set; }
        public required List<ChannelPermission> Permissions { get; set; }
    }
}
