using BreganTwitchBot.Infrastructure.Database.Context;
using BreganUtils.ProjectMonitor.Projects;
using Discord;
using Serilog;

namespace BreganTwitchBot.Domain.Data.DiscordBot
{
    public class Leaderboards
    {
        public static async Task UpdateTheRanks()
        {
            //Get the roles
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var rollList = new List<IRole>
            {
                guild.Roles.First(x => x.Name == "#1 Bits Leaderboard"),
                guild.Roles.First(x => x.Name == "#2 Bits Leaderboard"),
                guild.Roles.First(x => x.Name == "#3 Bits Leaderboard"),
                guild.Roles.First(x => x.Name == "#1 Subs Leaderboard"),
                guild.Roles.First(x => x.Name == "#2 Subs Leaderboard"),
                guild.Roles.First(x => x.Name == "#3 Subs Leaderboard")
            };

            await guild.DownloadUsersAsync();
            var memberList = guild.Users;

            //remove all the roles as its first day of month
            if (DateTime.UtcNow.Date.Day == 1)
            {
                using (var context = new DatabaseContext())
                {
                    var usersToUpdate = context.Users.Where(x => x.GiftedSubsThisMonth != 0 || x.BitsDonatedThisMonth != 0).ToList();

                    foreach (var user in usersToUpdate)
                    {
                        user.BitsDonatedThisMonth = 0;
                        user.GiftedSubsThisMonth = 0;
                    }

                    context.Users.UpdateRange(usersToUpdate);
                    context.SaveChanges();
                }

                //loop through the roles list and remove the role

                foreach (var role in rollList)
                {
                    var member = memberList.Where(x => x.Roles.ToList().Contains(role)).GetEnumerator();
                    member.MoveNext();
                    await member.Current.RemoveRoleAsync(role);
                }

                Log.Information("[Leaderboards] Leaderboards updated - roles removed");
                return;
            }

            //now to do the fun bit and add the rolls back lol

            //loop through the roles list and remove the role
            foreach (var role in rollList)
            {
                var member = memberList.Where(x => x.Roles.ToList().Contains(role)).GetEnumerator();
                member.MoveNext();

                if (member.Current == null)
                {
                    continue;
                }

                await member.Current.RemoveRoleAsync(role);
            }

            //Add them
            var bitsLb = new List<ulong>();

            using (var context = new DatabaseContext())
            {
                //Get the bits lb first
                var bits = context.Users.OrderByDescending(x => x.BitsDonatedThisMonth).Select(x => x.DiscordUserId).Take(3).ToList();

                foreach (var id in bits)
                {
                    bitsLb.Add(id);
                }

                //now the subs
                var subs = context.Users.OrderByDescending(x => x.GiftedSubsThisMonth).Select(x => x.DiscordUserId).Take(3).ToList();

                foreach (var id in subs)
                {
                    bitsLb.Add(id);
                }
            }

            var loops = 0;

            foreach (var userId in bitsLb)
            {
                if (userId == 0)
                {
                    loops++;
                    continue;
                }

                try
                {
                    if (userId == 0)
                    {
                        Log.Information($"[Leaderboards] User not linked");
                        continue;
                    }

                    var discordMember = guild.GetUser(userId);

                    await discordMember.AddRoleAsync(rollList[loops]);
                    Log.Information($"[Leaderboards] {discordMember.Username} given {rollList[loops].Name}");
                    loops++;
                }
                catch (Exception e)
                {
                    Log.Information($"[Leaderboards] Leaderboards error - {e}");
                    loops++;
                }
            }

            ProjectMonitorBreganTwitchBot.SendDiscordLeaderboardUpdate();
            Log.Information("[Leaderboards] Leaderboards updated");
        }
    }
}