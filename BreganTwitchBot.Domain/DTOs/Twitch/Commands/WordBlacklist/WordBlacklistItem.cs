using BreganTwitchBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.Commands.WordBlacklist
{
    public class WordBlacklistItem
    {
        public required string BroadcasterId { get; set; }
        public required string Word { get; set; }
        public required WordType WordType { get; set; }
    }
}
