using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class EventPredictionBase
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string PredictionTitle { get; set; }
        public required string PredictionId { get; set; }
        public required string PredictionStatus { get; set; }

    }
}
