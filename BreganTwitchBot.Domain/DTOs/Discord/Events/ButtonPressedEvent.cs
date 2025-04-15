using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Discord.Events
{
    public class ButtonPressedEvent : EventBase
    {
        public required string CustomId { get; set; }
    }
}
