using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Events
{
    public class Message
    {
        public static async Task HandleUserAddingOrUpdating(string username, string userId, bool isSubscriber)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();
                var uniqueUser = context.UniqueViewers.Where(x => x.Username == username).FirstOrDefault();

                if (uniqueUser == null)
                {
                    context.UniqueViewers.Add(new UniqueViewers
                    {
                        Username = username
                    });
                }

                if (user != null)
                {
                    user.TotalMessages++;
                    user.Username = username;
                    user.InStream = true;
                    user.IsSub = isSubscriber;
                    user.LastSeenDate = DateTime.UtcNow;
                }
                else
                {
                    context.Users.Add(new Users
                    {
                        BitsDonatedThisMonth = 0,
                        BossesDone = 0,
                        BossesPointsWon = 0,
                        CanUseOpenAi = false,
                        DailyPoints = new DailyPoints
                        {
                            CurrentStreak = 0,
                            DiscordDailyClaimed = false,
                            DiscordDailyStreak = 0,
                            DiscordDailyTotalClaims = 0,
                            HighestStreak = 0,
                            PointsClaimedThisStream = false,
                            PointsLastClaimed = DateTime.MinValue,
                            TotalPointsClaimed = 0,
                            TotalTimesClaimed = 0,
                            WeeklyClaimed = false,
                            MonthlyClaimed = false,
                            YearlyClaimed = false,
                            CurrentWeeklyStreak = 0,
                            CurrentMonthlyStreak = 0,
                            CurrentYearlyStreak = 0,
                            HighestWeeklyStreak = 0,
                            HighestMonthlyStreak = 0,
                            HighestYearlyStreak = 0,
                            TotalWeeklyClaimed = 0,
                            TotalMonthlyClaimed = 0,
                            TotalYearlyClaimed = 0,
                        },
                        DiscordUserId = 0,
                        DiscordUserStats = new DiscordUserStats
                        {
                            DiscordLevel = 0,
                            DiscordLevelUpNotifsEnabled = true,
                            DiscordXp = 0,
                            PrestigeLevel = 0,
                        },
                        GiftedSubsThisMonth = 0,
                        InStream = true,
                        IsSub = isSubscriber,
                        IsSuperMod = false,
                        LastSeenDate = DateTime.UtcNow,
                        MarblesWins = 0,
                        Points = 100,
                        TimeoutStrikes = 0,
                        TotalMessages = 1,
                        TwitchUserId = userId,
                        UserGambleStats = new UserGambleStats
                        {
                            JackpotWins = 0,
                            PointsGambled = 0,
                            PointsLost = 0,
                            PointsWon = 0,
                            SmorcWins = 0,
                            Tier1Wins = 0,
                            Tier2Wins = 0,
                            Tier3Wins = 0,
                            TotalSpins = 0
                        },
                        Username = username,
                        WarnStrikes = 0,
                        Watchtime = new Infrastructure.Database.Models.Watchtime
                        {
                            Rank1Applied = false,
                            Rank2Applied = false,
                            Rank3Applied = false,
                            Rank4Applied = false,
                            Rank5Applied = false,
                            MinutesInStream = 0,
                            MinutesWatchedThisMonth = 0,
                            MinutesWatchedThisStream = 0,
                            MinutesWatchedThisWeek = 0,
                        }
                    });
                }

                context.SaveChanges();
            }
        }
    }
}
