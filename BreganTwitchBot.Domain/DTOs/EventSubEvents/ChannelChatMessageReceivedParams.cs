using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.EventSubEvents
{
    public class ChannelChatMessageReceivedParams
    {
        public required string BroadcasterChannelName { get; set; }
        public required string BroadcasterChannelId { get; set; }
        public required string ChatterChannelName { get; set; }
        public required string ChatterChannelId { get; set; }
        public required string Message { get; set; }
        public required string[] MessageParts { get; set; }
        public required string MessageId { get; set; }
        public required bool IsMod { get; set; }
        public required bool IsVip { get; set; }
        public required bool IsSub { get; set; }
        public required bool IsBroadcaster { get; set; }
    }
}
