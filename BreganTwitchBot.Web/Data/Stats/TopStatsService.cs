using BreganTwitchBot.Data;

namespace BreganTwitchBot.Web.Data.Stats
{
    public class TopStatsService
    {
        public TopStats GetStats()
        {
            using(var context = new DatabaseContext())
            {
                var topStats = new TopStats();

                topStats.TotalPointsInSystem = context.Users.Where(x => x.Points != 0).Sum(x => x.Points);

                var minutes = context.Users.Where(x => x.MinutesInStream != 0).Sum(x => x.MinutesInStream);
                topStats.TotalMinutesWatched = $"{minutes:N0} minutes ({Math.Round((double)minutes / 60, 2):N0} hours)";
                topStats.UsersInStream = context.Users.Where(x => x.InStream == true).Count();
                topStats.TotalUsersRegistered = context.Users.Count();

                return topStats;
            }
        }
    }
}
