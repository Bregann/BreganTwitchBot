using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public partial class EventBase
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string ChatterChannelName { get; set; }
        public required string ChatterChannelId { get; set; }
    }
}
