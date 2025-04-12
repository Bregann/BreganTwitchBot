using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.Commands.WordBlacklist
{
    public class WordBlacklistUser
    {
        public required string UserId { get; set; }
        public required string BroadcasterId { get; set; }
        public required DateTime AddedAt { get; set; }

    }
}
