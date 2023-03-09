using BreganTwitchBot.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Web.Data.Stats
{
    public class TopStatsService
    {
        public TopStats GetStats()
        {
            using (var context = new DatabaseContext())
            {
                var topStats = new TopStats();

                topStats.TotalPointsInSystem = context.Users.Where(x => x.Points != 0).Sum(x => x.Points);

                var minutes = context.Users.Include(x => x.Watchtime).Where(x => x.Watchtime.MinutesInStream != 0).Sum(x => x.Watchtime.MinutesInStream);
                topStats.TotalMinutesWatched = $"{minutes:N0} minutes ({Math.Round((double)minutes / 60, 2):N0} hours)";
                topStats.UsersInStream = context.Users.Where(x => x.InStream == true).Count();
                topStats.TotalUsersRegistered = context.Users.Count();

                return topStats;
            }
        }
    }
}