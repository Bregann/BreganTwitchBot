using BreganTwitchBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelGiftSubParams : EventBase
    {
        public required int Total { get; set; }
        public int? CumulativeTotal { get; set; }
        public required SubTierEnum SubTier { get; set; }
        public required bool IsAnonymous { get; set; }
    }
}
