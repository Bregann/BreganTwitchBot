using BreganTwitchBot.Data;
using BreganTwitchBot.Web.Data.Leaderboards.Enums;

namespace BreganTwitchBot.Web.Data.Leaderboards
{
    public class Leaderboards
    {
        public long Position { get; set; }
        public string Username { get; set; }
        public long Amount { get; set; }
    }
}
