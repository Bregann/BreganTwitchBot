using BreganTwitchBot.Infrastructure.Database.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Helpers
{
    public class PointsHelper
    {
        public static long GetUserPoints(string userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return 0;
                }
                return user.Points;
            }
        }

        public static async Task AddUserPoints(string userId, long pointsToAdd)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return;
                }

                user.Points += pointsToAdd;

                await context.SaveChangesAsync();
            }
        }

        public static async Task RemoveUserPoints(string userId, long pointsToRemove)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return;
                }

                user.Points -= pointsToRemove;

                await context.SaveChangesAsync();
            }
        }
    }
}
