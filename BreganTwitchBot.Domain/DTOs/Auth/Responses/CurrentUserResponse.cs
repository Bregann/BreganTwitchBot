namespace BreganTwitchBot.Domain.DTOs.Auth.Responses
{
    public class CurrentUserResponse
    {
        public required string TwitchUserId { get; set; }
        public required string TwitchUsername { get; set; }
        public string? TwitchDisplayName { get; set; }
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Channels this user broadcasts, so the UI knows where to offer admin
        /// </summary>
        public required List<string> BroadcasterOfChannels { get; set; }
    }
}
