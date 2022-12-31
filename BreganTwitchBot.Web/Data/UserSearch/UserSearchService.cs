using BreganTwitchBot.Infrastructure.Database.Context;

namespace BreganTwitchBot.Web.Data.UserSearch
{
    public class UserSearchService
    {
        public UserSearchData? GetUserDataByTwitchUsername(string twitchUsername)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == twitchUsername).FirstOrDefault();

                if (user == null)
                {
                    return null;
                }

                return new UserSearchData
                {
                    DiscordDailyTotalClaims = user.DiscordDailyTotalClaims,
                    DiscordLevel = user.DiscordLevel,
                    DiscordXP = user.DiscordXp,
                    LastSeenDate = user.LastSeenDate,
                    TwitchBossesDone = user.BossesDone,
                    CurrentStreak = user.CurrentStreak,
                    HighestStreak = user.HighestStreak,
                    JackpotWins = user.JackpotWins,
                    MarblesWins = user.MarblesWins,
                    MessagesSent = user.TotalMessages,
                    MinutesInStream = user.MinutesInStream,
                    MinutesWatchedThisMonth = user.MinutesWatchedThisMonth,
                    MinutesWatchedThisStream = user.MinutesWatchedThisStream,
                    MinutesWatchedThisWeek = user.MinutesWatchedThisWeek,
                    PointsGambled = user.PointsGambled,
                    PointsLost = user.PointsLost,
                    PointsWon = user.PointsWon,
                    SmorcWins = user.SmorcWins,
                    Tier1Wins = user.Tier1Wins,
                    Tier2Wins = user.Tier2Wins,
                    Tier3Wins = user.Tier3Wins,
                    TotalPointsClaimed = user.TotalPointsClaimed,
                    TotalSpins = user.TotalSpins,
                    TotalTimesClaimed = user.TotalTimesClaimed,
                    TwitchBossesPointsWon = user.BossesPointsWon,
                    TwitchUsername = user.Username,
                    UserId = user.TwitchUserId
                };
            }
        }
    }
}