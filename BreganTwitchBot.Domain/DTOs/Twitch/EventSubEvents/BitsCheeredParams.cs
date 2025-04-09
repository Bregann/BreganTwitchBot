using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class BitsCheeredParams : EventBase
    {
        public required int Amount { get; set; }
        public required string Message { get; set; }
        public required bool IsAnonymous { get; set; }
    }
}
