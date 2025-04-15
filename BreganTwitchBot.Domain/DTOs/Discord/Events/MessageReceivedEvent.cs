using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Discord.Events
{
    public class MessageReceivedEvent : EventBase
    {
        public required ulong ChannelId { get; set; }
        public required string ChannelName { get; set; }
        public required ulong MessageId { get; set; }
        public required string MessageContent { get; set; }
        public required bool HasAttachments { get; set; }
    }
}
