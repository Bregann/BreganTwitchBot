using BreganTwitchBot.Infrastructure.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Api.Dtos
{
    public class GetSubathonLeaderboardsDto
    {
        public required List<Subathon> SubsLeaderboard { get; set; }
        public required List<Subathon> BitsLeaderboard { get; set; }
    }
}
