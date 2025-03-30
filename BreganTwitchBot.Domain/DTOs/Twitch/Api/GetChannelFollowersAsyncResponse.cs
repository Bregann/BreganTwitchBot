using Newtonsoft.Json;


namespace BreganTwitchBot.Domain.DTOs.Twitch.Api
{
    public class GetChannelFollowersAsyncResponse
    {
        public required List<ChannelFollower> Followers { get; set; }
        public required int Total { get; set; }
    }

    public class ChannelFollower
    {
        public required string UserId { get; set; }
        public required string UserLogin { get; set; }
        public required string UserName { get; set; }
        public required string FollowedAt { get; set; }
    }
}
