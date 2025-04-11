using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelPollEndParams
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string PollTitle { get; set; }
        public required PollEndChoices[] PollEndResults { get; set; }
    }

    public class PollEndChoices
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required int Votes { get; set; }
        public required int ChannelPointsVotes { get; set; }
        public required int BitsVotes { get; set; }
    }
}
