using BreganTwitchBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelResubscribeParams : EventBase
    {
        public required string Message { get; set; }
        public int? StreakMonths { get; set; }
        public required int CumulativeMonths { get; set; }
        public required SubTierEnum SubTier { get; set; }
    }
}
