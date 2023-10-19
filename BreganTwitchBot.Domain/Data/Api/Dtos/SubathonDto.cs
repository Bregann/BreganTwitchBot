using BreganTwitchBot.Infrastructure.Database.Models;

namespace BreganTwitchBot.Domain.Data.Api.Dtos
{
    public class GetSubathonLeaderboardsDto
    {
        public required List<Subathon> SubsLeaderboard { get; set; }
        public required List<Subathon> BitsLeaderboard { get; set; }
    }

    public class GetSubathonTimeLeftDto
    {
        public required double SecondsLeft { get; set; }
        public required string TimeExtended { get; set; }
        public required bool PlaySound { get; set; }
        public required bool TimeUpdated { get; set; }
    }
}
