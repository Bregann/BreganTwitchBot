using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Events
{
    public class UserPressence
    {
        public static async Task HandleUserLeftEvent(string username)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

                if (user != null)
                {
                    user.InStream = false;
                    await context.SaveChangesAsync();
                }
            }

            Log.Information($"[User Left] {username} left the stream");
        }
    }
}