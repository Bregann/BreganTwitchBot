using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelRaidParams
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string RaidingChannelName { get; set; }
        public required string RaidingChannelId { get; set; }
        public required int Viewers { get; set; }
    }
}
