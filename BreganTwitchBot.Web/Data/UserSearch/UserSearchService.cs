using BreganTwitchBot.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Web.Data.UserSearch
{
    public class UserSearchService
    {
        public UserSearchData? GetUserDataByTwitchUsername(string twitchUsername)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Include(x => x.DailyPoints).Include(x => x.DiscordUserStats).Include(x => x.UserGambleStats).Include(x => x.Watchtime).Where(x => x.Username == twitchUsername).FirstOrDefault();

                if (user == null)
                {
                    return null;
                }

                return new UserSearchData
                {
                    DiscordDailyTotalClaims = user.DailyPoints.DiscordDailyTotalClaims,
                    DiscordLevel = user.DiscordUserStats.DiscordLevel,
                    DiscordXP = user.DiscordUserStats.DiscordXp,
                    LastSeenDate = user.LastSeenDate,
                    TwitchBossesDone = user.BossesDone,
                    CurrentStreak = user.DailyPoints.CurrentStreak,
                    HighestStreak = user.DailyPoints.HighestStreak,
                    JackpotWins = user.UserGambleStats.JackpotWins,
                    MarblesWins = user.MarblesWins,
                    MessagesSent = user.TotalMessages,
                    MinutesInStream = user.Watchtime.MinutesInStream,
                    MinutesWatchedThisMonth = user.Watchtime.MinutesWatchedThisMonth,
                    MinutesWatchedThisStream = user.Watchtime.MinutesWatchedThisStream,
                    MinutesWatchedThisWeek = user.Watchtime.MinutesWatchedThisWeek,
                    PointsGambled = user.UserGambleStats.PointsGambled,
                    PointsLost = user.UserGambleStats.PointsLost,
                    PointsWon = user.UserGambleStats.PointsWon,
                    SmorcWins = user.UserGambleStats.SmorcWins,
                    Tier1Wins = user.UserGambleStats.Tier1Wins,
                    Tier2Wins = user.UserGambleStats.Tier2Wins,
                    Tier3Wins = user.UserGambleStats.Tier3Wins,
                    TotalPointsClaimed = user.DailyPoints.TotalPointsClaimed,
                    TotalSpins = user.UserGambleStats.TotalSpins,
                    TotalTimesClaimed = user.DailyPoints.TotalTimesClaimed,
                    TwitchBossesPointsWon = user.BossesPointsWon,
                    TwitchUsername = user.Username,
                    UserId = user.TwitchUserId
                };
            }
        }
    }
}