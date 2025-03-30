using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.Api
{
    public class GetChattersResult
    {
        public required List<Chatters> Chatters { get; set; }
    }

    public class Chatters
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
    }
}
