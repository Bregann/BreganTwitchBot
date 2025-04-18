namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelPollBeginParams
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string PollTitle { get; set; }
        public required PollStartChoices[] PollChoices { get; set; }
    }

    public class PollStartChoices
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
    }
}
