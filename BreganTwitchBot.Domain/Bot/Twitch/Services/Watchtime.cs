using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.Linking;
using BreganTwitchBot.Infrastructure.Database.Models;
using BreganUtils.ProjectMonitor.Projects;
using Serilog;
using BreganTwitchBot.Domain.Data.TwitchBot.Stats;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;

namespace BreganTwitchBot.Domain.Bot.Twitch.Services
{
    public class Watchtime
    {
        public static async Task UpdateUserWatchtime()
        {
            if (!AppConfig.StreamerLive)
            {
                return;
            }

            Log.Information("[User Update] User update started");

            using (var context = new DatabaseContext())
            {
                var usersBeingUpdated = context.Users.Where(x => x.InStream == true).ToList();
                int totalPointsAdded = 0;

                foreach (var user in usersBeingUpdated)
                {
                    user.MinutesInStream++;
                    user.MinutesWatchedThisStream++;
                    user.MinutesWatchedThisWeek++;
                    user.MinutesWatchedThisMonth++;
                    user.LastSeenDate = DateTime.UtcNow;

                    if (user.IsSub)
                    {
                        user.Points += 300;
                        totalPointsAdded += 300;
                    }
                    else
                    {
                        user.Points += 100;
                        totalPointsAdded += 100;
                    }
                }

                context.Users.UpdateRange(usersBeingUpdated);
                context.SaveChanges();

                StreamStatsService.UpdateStreamStat(totalPointsAdded, StatTypes.PointsGainedWatching);
                ProjectMonitorBreganTwitchBot.SendLastHoursUpdateUpdate();
                ProjectMonitorBreganTwitchBot.SendUsersInStreamUpdate(usersBeingUpdated.Count);

                Log.Information($"[User Update] User update complete. {usersBeingUpdated.Count} users updated. {totalPointsAdded} points added");
                await CheckForRankUps();
            }
        }

        //todo: refactor this method
        private static async Task CheckForRankUps()
        {
            Log.Information("[Discord Ranks] Discord ranks started");

            var rank1Users = new List<Users>();
            var rank2Users = new List<Users>();
            var rank3Users = new List<Users>();
            var rank4Users = new List<Users>();
            var rank5Users = new List<Users>();

            using (var context = new DatabaseContext())
            {
                rank1Users = context.Users.Where(x => x.MinutesInStream == 60 && x.Rank1Applied == false).ToList();
                rank2Users = context.Users.Where(x => x.MinutesInStream == 1500 && x.Rank2Applied == false).ToList();
                rank3Users = context.Users.Where(x => x.MinutesInStream == 6000 && x.Rank3Applied == false).ToList();
                rank4Users = context.Users.Where(x => x.MinutesInStream == 15000 && x.Rank4Applied == false).ToList();
                rank5Users = context.Users.Where(x => x.MinutesInStream == 30000 && x.Rank5Applied == false).ToList();
            }

            var rankUps = 0;
            var ranksApplied = 0;

            foreach (var user in rank1Users)
            {
                ranksApplied++;

                if (user.DiscordUserId == 0)
                {
                    if (rankUps >= 1)
                    {
                        user.Rank1Applied = true;
                        StreamStatsService.UpdateStreamStat(1, StatTypes.DiscordRanksEarnt);
                        return;
                    }
                    else
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the Melvin Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                        user.Rank1Applied = true;
                    }
                }
                else
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the Melvin Rank on Discord! Your rank has been applied!");
                    await DiscordLinking.ApplyDiscordRankUp(user);
                    user.Rank1Applied = true;
                }
            }

            foreach (var user in rank2Users)
            {
                ranksApplied++;

                if (user.DiscordUserId == 0)
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the WOT Crew Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                    user.Rank2Applied = true;
                }
                else
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the WOT Crew Rank on Discord! Your rank has been applied!");
                    await DiscordLinking.ApplyDiscordRankUp(user);
                    user.Rank2Applied = true;
                }
            }

            foreach (var user in rank3Users)
            {
                ranksApplied++;

                if (user.DiscordUserId == 0)
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the BLOCKS Crew Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                    user.Rank3Applied = true;
                }
                else
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the BLOCKS Crew Rank on Discord! Your rank has been applied!");
                    await DiscordLinking.ApplyDiscordRankUp(user);
                    user.Rank3Applied = true;
                }
            }

            foreach (var user in rank4Users)
            {
                ranksApplied++;

                if (user.DiscordUserId == 0)
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The Name of Legends Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                    user.Rank4Applied = true;
                }
                else
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The Name of Legends Rank on Discord! Your rank has been applied!");
                    await DiscordLinking.ApplyDiscordRankUp(user);
                    user.Rank4Applied = true;
                }
            }

            foreach (var user in rank5Users)
            {
                ranksApplied++;

                if (user.DiscordUserId == 0)
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The King of The Stream Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                    user.Rank5Applied = true;
                }
                else
                {
                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The King of The Stream Rank on Discord! Your rank has been applied!");
                    await DiscordLinking.ApplyDiscordRankUp(user);
                    user.Rank5Applied = true;
                }
            }

            if (ranksApplied != 0)
            {
                StreamStatsService.UpdateStreamStat(ranksApplied, StatTypes.DiscordRanksEarnt);

                using (var context = new DatabaseContext())
                {
                    context.Users.UpdateRange(rank1Users);
                    context.Users.UpdateRange(rank2Users);
                    context.Users.UpdateRange(rank3Users);
                    context.Users.UpdateRange(rank4Users);
                    context.Users.UpdateRange(rank5Users);
                    context.SaveChanges();
                }
            }

            Log.Information("[Discord Ranks] Discord ranks checked");
        }
    }
}