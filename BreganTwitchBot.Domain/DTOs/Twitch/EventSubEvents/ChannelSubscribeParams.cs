using BreganTwitchBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelSubscribeParams : EventBase
    {
        public required SubTierEnum SubTier { get; set; }
        public required bool IsGift { get; set; }
    }
}
