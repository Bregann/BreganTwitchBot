using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Twitch.Commands.Points
{
    public class GetPointsResponse
    {
        public required string TwitchUsername { get; set; }
        public required int Points { get; set; }
    }
}
