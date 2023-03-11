using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Linking;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using BreganUtils.ProjectMonitor.Projects;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot
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
                var usersBeingUpdated = context.Users.Include(x => x.Watchtime).Where(x => x.InStream == true).ToList();
                int totalPointsAdded = 0;

                foreach (var user in usersBeingUpdated)
                {
                    user.Watchtime.MinutesInStream++;
                    user.Watchtime.MinutesWatchedThisStream++;
                    user.Watchtime.MinutesWatchedThisWeek++;
                    user.Watchtime.MinutesWatchedThisMonth++;
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

                context.SaveChanges();

                await StreamStatsService.UpdateStreamStat(totalPointsAdded, StatTypes.PointsGainedWatching);
                ProjectMonitorBreganTwitchBot.SendLastHoursUpdateUpdate();
                ProjectMonitorBreganTwitchBot.SendUsersInStreamUpdate(usersBeingUpdated.Count);

                Log.Information($"[User Update] User update complete. {usersBeingUpdated.Count} users updated. {totalPointsAdded} points added");
                await CheckForRankUps();
            }
        }

        private static async Task CheckForRankUps()
        {
            Log.Information("[Discord Ranks] Discord ranks started");

            using (var context = new DatabaseContext())
            {
                var rank1Users = context.Users.Include(x => x.Watchtime).Where(x => x.Watchtime.MinutesInStream == 60 && x.Watchtime.Rank1Applied == false).ToList();
                var rank2Users = context.Users.Include(x => x.Watchtime).Where(x => x.Watchtime.MinutesInStream == 1500 && x.Watchtime.Rank2Applied == false).ToList();
                var rank3Users = context.Users.Include(x => x.Watchtime).Where(x => x.Watchtime.MinutesInStream == 6000 && x.Watchtime.Rank3Applied == false).ToList();
                var rank4Users = context.Users.Include(x => x.Watchtime).Where(x => x.Watchtime.MinutesInStream == 15000 && x.Watchtime.Rank4Applied == false).ToList();
                var rank5Users = context.Users.Include(x => x.Watchtime).Where(x => x.Watchtime.MinutesInStream == 30000 && x.Watchtime.Rank5Applied == false).ToList();
            
                var rankUps = 0;
                var ranksApplied = 0;

                foreach (var user in rank1Users)
                {
                    ranksApplied++;

                    if (user.DiscordUserId == 0)
                    {
                        if (rankUps >= 1)
                        {
                            user.Watchtime.Rank1Applied = true;
                            await StreamStatsService.UpdateStreamStat(1, StatTypes.DiscordRanksEarnt);
                            continue;
                        }
                        else
                        {
                            TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the Melvin Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                            user.Watchtime.Rank1Applied = true;
                        }
                    }
                    else
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the Melvin Rank on Discord! Your rank has been applied!");
                        await DiscordLinking.ApplyDiscordRankUp(user);
                        user.Watchtime.Rank1Applied = true;
                    }
                }

                foreach (var user in rank2Users)
                {
                    ranksApplied++;

                    if (user.DiscordUserId == 0)
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the WOT Crew Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                        user.Watchtime.Rank2Applied = true;
                    }
                    else
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the WOT Crew Rank on Discord! Your rank has been applied!");
                        await DiscordLinking.ApplyDiscordRankUp(user);
                        user.Watchtime.Rank2Applied = true;
                    }
                }

                foreach (var user in rank3Users)
                {
                    ranksApplied++;

                    if (user.DiscordUserId == 0)
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the BLOCKS Crew Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                        user.Watchtime.Rank3Applied = true;
                    }
                    else
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the BLOCKS Crew Rank on Discord! Your rank has been applied!");
                        await DiscordLinking.ApplyDiscordRankUp(user);
                        user.Watchtime.Rank3Applied = true;
                    }
                }

                foreach (var user in rank4Users)
                {
                    ranksApplied++;

                    if (user.DiscordUserId == 0)
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The Name of Legends Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                        user.Watchtime.Rank4Applied = true;
                    }
                    else
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The Name of Legends Rank on Discord! Your rank has been applied!");
                        await DiscordLinking.ApplyDiscordRankUp(user);
                        user.Watchtime.Rank4Applied = true;
                    }
                }

                foreach (var user in rank5Users)
                {
                    ranksApplied++;

                    if (user.DiscordUserId == 0)
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The King of The Stream Rank on discord! Make sure to join the discord ( https://discord.gg/jAjKtHZ ) and connect your Twitch account!");
                        user.Watchtime.Rank5Applied = true;
                    }
                    else
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The King of The Stream Rank on Discord! Your rank has been applied!");
                        await DiscordLinking.ApplyDiscordRankUp(user);
                        user.Watchtime.Rank5Applied = true;
                    }
                }

                if (ranksApplied != 0)
                {
                    await StreamStatsService.UpdateStreamStat(ranksApplied, StatTypes.DiscordRanksEarnt);
                    await context.SaveChangesAsync();
                }

                Log.Information("[Discord Ranks] Discord ranks checked");
            }
        }
    }
}