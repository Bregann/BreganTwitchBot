﻿using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
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
        private static List<Users> _usersBeingUpdated = new();
        public static async Task UpdateUserWatchtime()
        {
            if (!AppConfig.StreamerLive)
            {
                return;
            }

            Log.Information("[User Update] User update started");
            _usersBeingUpdated.Clear();

            using (var context = new DatabaseContext())
            {
                _usersBeingUpdated = context.Users.Include(x => x.Watchtime).Where(x => x.InStream == true).ToList();
                int totalPointsAdded = 0;

                foreach (var user in _usersBeingUpdated)
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

                StreamStatsService.UpdateStreamStat(totalPointsAdded, StatTypes.PointsGainedWatching);
                ProjectMonitorBreganTwitchBot.SendLastHoursUpdateUpdate();
                ProjectMonitorBreganTwitchBot.SendUsersInStreamUpdate(_usersBeingUpdated.Count);

                Log.Information($"[User Update] User update complete. {_usersBeingUpdated.Count} users updated. {totalPointsAdded} points added");
                await CheckForRankUps();

                _usersBeingUpdated.Clear();
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

                    if (rankUps >= 1)
                    {
                        user.Watchtime.Rank1Applied = true;
                        StreamStatsService.UpdateStreamStat(1, StatTypes.DiscordRanksEarnt);
                        continue;
                    }
                    else
                    {
                        TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the Pirate Kitty rank for watching 1 hour!");
                        user.Watchtime.Rank1Applied = true;
                    }
                }

                foreach (var user in rank2Users)
                {
                    ranksApplied++;

                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the High Tempo rank for watching 25 hours!");
                    user.Watchtime.Rank2Applied = true;
                }

                foreach (var user in rank3Users)
                {
                    ranksApplied++;

                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the Never Lucky rank for watching 100 hours!");
                    user.Watchtime.Rank3Applied = true;
                }

                foreach (var user in rank4Users)
                {
                    ranksApplied++;

                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the Bob Enjoyer rank for watching 250 hours!");
                    user.Watchtime.Rank4Applied = true;
                }

                foreach (var user in rank5Users)
                {
                    ranksApplied++;

                    TwitchHelper.SendMessage($"@{user.Username} => Hey congrats you got the The Ultimate Fireball Champion Rank for watching 500 hours!");
                    user.Watchtime.Rank5Applied = true;
                }

                if (ranksApplied != 0)
                {
                    StreamStatsService.UpdateStreamStat(ranksApplied, StatTypes.DiscordRanksEarnt);
                    await context.SaveChangesAsync();
                }

                Log.Information("[Discord Ranks] Discord ranks checked");
            }
        }
    }
}