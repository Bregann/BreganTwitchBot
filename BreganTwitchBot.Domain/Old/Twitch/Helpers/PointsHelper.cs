using BreganTwitchBot.Infrastructure.Database.Context;

namespace BreganTwitchBot.Domain.Bot.Twitch.Helpers
{
    public class PointsHelper
    {
        public static long GetUserPoints(string username)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

                if (user == null)
                {
                    return 0;
                }
                return user.Points;
            }
        }

        public static void AddUserPoints(string username, long pointsToAdd)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

                if (user == null)
                {
                    return;
                }

                user.Points += pointsToAdd;

                context.SaveChanges();
            }
        }

        public static void RemoveUserPoints(string username, long pointsToRemove)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

                if (user == null)
                {
                    return;
                }

                user.Points -= pointsToRemove;

                context.SaveChanges();
            }
        }
    }
}