using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.DomainTests.Helpers
{
    public class DatabaseSeedHelper
    {
        public const string Channel1BroadcasterTwitchChannelId = "123";
        public const string Channel1BotTwitchChannelName = "coolbotname";
        public const string Channel1BroadcasterTwitchChannelName = "coolstreamername";
        public const string Channel1ChannelCurrencyName = "CoolCurrencyName";

        public const string Channel2BroadcasterTwitchChannelId = "12345";
        public const string Channel2BotTwitchChannelName = "coolbotname2";
        public const string Channel2BroadcasterTwitchChannelName = "coolstreamername2";
        public const string Channel2ChannelCurrencyName = "CoolCurrencyName2";

        public const string Channel1User1TwitchUserId = "456";
        public const string Channel1User1TwitchUsername = "cooluser";

        public const string Channel1User2TwitchUserId = "789";
        public const string Channel1User2TwitchUsername = "cooluser2";

        public const string Channel2User1TwitchUserId = "999";
        public const string Channel2User1TwitchUsername = "cooluser3";

        public const string Channel1SuperModUserTwitchUserId = "1111";
        public const string Channel1SuperModUserTwitchUsername = "supermoduser";

        public const string SeededChannel1BannedWord = "seededbannedword";
        public const string SeededChannel1TempBanWord = "seededtempbanword";
        public const string SeededChannel2BannedWord = "seededbannedword";

        public const ulong DiscordGuildId = 12345;


        public static async Task SeedDatabase(AppDbContext context)
        {
            var channel = new Channel
            {
                BotTwitchChannelOAuthToken = "",
                BroadcasterTwitchChannelOAuthToken = "",
                BotTwitchChannelId = "",
                BotTwitchChannelRefreshToken = "",
                BroadcasterTwitchChannelRefreshToken = "",
                BotTwitchChannelName = Channel1BotTwitchChannelName,
                BroadcasterTwitchChannelId = Channel1BroadcasterTwitchChannelId,
                BroadcasterTwitchChannelName = Channel1BroadcasterTwitchChannelName,
                DiscordEnabled = true,
                DiscordGuildId = DiscordGuildId
            };

            var channel2 = new Channel
            {
                BotTwitchChannelOAuthToken = "",
                BroadcasterTwitchChannelOAuthToken = "",
                BotTwitchChannelId = "",
                BotTwitchChannelRefreshToken = "",
                BroadcasterTwitchChannelRefreshToken = "",
                BotTwitchChannelName = Channel2BotTwitchChannelName,
                BroadcasterTwitchChannelId = Channel2BroadcasterTwitchChannelId,
                BroadcasterTwitchChannelName = Channel2BroadcasterTwitchChannelName,
                DiscordEnabled = false
            };

            await context.Channels.AddAsync(channel);
            await context.Channels.AddAsync(channel2);
            await context.SaveChangesAsync();

            await context.ChannelConfig.AddAsync(new ChannelConfig
            {
                ChannelId = channel.Id,
                DailyPointsCollectingAllowed = false,
                LastDailyPointsAllowed = DateTime.UtcNow.AddDays(-1),
                StreamAnnounced = false,
                SubathonActive = false,
                ChannelCurrencyName = Channel1ChannelCurrencyName,
                CurrencyPointCap = 1000,
                StreamHappenedThisWeek = false,
                SubathonTime = TimeSpan.FromHours(1),
                LastStreamStartDate = DateTime.UtcNow.AddDays(-2),
                LastStreamEndDate = DateTime.UtcNow.AddDays(-1),
                BroadcasterLive = true
            });

            await context.ChannelConfig.AddAsync(new ChannelConfig
            {
                ChannelId = channel2.Id,
                DailyPointsCollectingAllowed = false,
                LastDailyPointsAllowed = DateTime.UtcNow,
                StreamAnnounced = false,
                SubathonActive = false,
                ChannelCurrencyName = Channel2ChannelCurrencyName,
                CurrencyPointCap = 2000,
                StreamHappenedThisWeek = false,
                SubathonTime = TimeSpan.FromHours(1),
                LastStreamStartDate = DateTime.UtcNow.AddDays(-2),
                LastStreamEndDate = DateTime.UtcNow.AddDays(-1),
                BroadcasterLive = true
            });

            await context.SaveChangesAsync();

            var channelUser = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel1User1TwitchUserId,
                TwitchUsername = Channel1User1TwitchUsername,
                LastSeen = DateTime.UtcNow
            };

            var channelUser2 = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel1User2TwitchUserId,
                TwitchUsername = Channel1User2TwitchUsername,
                LastSeen = DateTime.UtcNow
            };

            var channelUser3 = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel2User1TwitchUserId,
                TwitchUsername = Channel2User1TwitchUsername,
                LastSeen = DateTime.UtcNow
            };

            var superModUser = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel1SuperModUserTwitchUserId,
                TwitchUsername = Channel1SuperModUserTwitchUsername,
                LastSeen = DateTime.UtcNow
            };

            await context.ChannelUsers.AddAsync(channelUser);
            await context.ChannelUsers.AddAsync(channelUser2);
            await context.ChannelUsers.AddAsync(channelUser3);
            await context.ChannelUsers.AddAsync(superModUser);
            await context.SaveChangesAsync();

            await context.ChannelUserData.AddAsync(new ChannelUserData
            {
                ChannelUserId = channelUser.Id,
                Points = 100,
                ChannelId = channel.Id,
                InStream = false,
                IsSub = false,
                IsSuperMod = false,
                TimeoutStrikes = 0,
                WarnStrikes = 0
            });

            await context.ChannelUserData.AddAsync(new ChannelUserData
            {
                ChannelUserId = channelUser2.Id,
                Points = 73,
                ChannelId = channel.Id,
                InStream = false,
                IsSub = false,
                IsSuperMod = false,
                TimeoutStrikes = 0,
                WarnStrikes = 0
            });

            await context.ChannelUserData.AddAsync(new ChannelUserData
            {
                ChannelUserId = channelUser3.Id,
                Points = 5555,
                ChannelId = channel2.Id,
                InStream = false,
                IsSub = false,
                IsSuperMod = false,
                TimeoutStrikes = 0,
                WarnStrikes = 0
            });            
            
            await context.ChannelUserData.AddAsync(new ChannelUserData
            {
                ChannelUserId = superModUser.Id,
                Points = 5555,
                ChannelId = channel.Id,
                InStream = false,
                IsSub = false,
                IsSuperMod = true,
                TimeoutStrikes = 0,
                WarnStrikes = 0
            });

            await context.SaveChangesAsync();

            await context.ChannelUserStats.AddAsync(new ChannelUserStats
            {
                BitsDonatedThisMonth = 0,
                BossesDone = 0,
                BossesPointsWon = 0,
                GiftedSubsThisMonth = 0,
                MarblesWins = 0,
                TotalMessages = 0,
                ChannelId = channel.Id,
                ChannelUserId = channelUser.Id
            });


            await context.CustomCommands.AddAsync(new CustomCommand
            {
                CommandName = "!oncooldown",
                ChannelId = channel.Id,
                CommandText = "This is a test command that is on cooldown",
                LastUsed = DateTime.UtcNow.AddDays(1),
                TimesUsed = 0
            });
            
            await context.CustomCommands.AddAsync(new CustomCommand
            {
                CommandName = "!readytouse",
                ChannelId = channel.Id,
                CommandText = "This is a test command ready to use",
                LastUsed = DateTime.UtcNow.AddSeconds(-30),
                TimesUsed = 0
            });

            await context.CustomCommands.AddAsync(new CustomCommand
            {
                CommandName = "!2ndchannel",
                ChannelId = channel2.Id,
                CommandText = "This is a test command on the other channel",
                LastUsed = DateTime.UtcNow.AddSeconds(-30),
                TimesUsed = 0
            });

            await context.SaveChangesAsync();

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 1,
                HighestStreak = 1,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Daily,
                TotalTimesClaimed = 1
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 2,
                HighestStreak = 2,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Weekly,
                TotalTimesClaimed = 2
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 3,
                HighestStreak = 3,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Monthly,
                TotalTimesClaimed = 3
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 4,
                HighestStreak = 4,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Yearly,
                TotalTimesClaimed = 4
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser2.Id,
                CurrentStreak = 2,
                HighestStreak = 2,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Daily,
                TotalTimesClaimed = 2
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser2.Id,
                CurrentStreak = 3,
                HighestStreak = 3,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Weekly,
                TotalTimesClaimed = 3
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser2.Id,
                CurrentStreak = 4,
                HighestStreak = 4,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Monthly,
                TotalTimesClaimed = 4
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser2.Id,
                CurrentStreak = 5,
                HighestStreak = 5,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Yearly,
                TotalTimesClaimed = 5
            });


            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel2.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 1,
                HighestStreak = 1,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Daily,
                TotalTimesClaimed = 1
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel2.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 2,
                HighestStreak = 2,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Weekly,
                TotalTimesClaimed = 2
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel2.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 3,
                HighestStreak = 3,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Monthly,
                TotalTimesClaimed = 3
            });

            await context.TwitchDailyPoints.AddAsync(new TwitchDailyPoints
            {
                ChannelId = channel2.Id,
                PointsLastClaimed = DateTime.UtcNow,
                TotalPointsClaimed = 0,
                ChannelUserId = channelUser.Id,
                CurrentStreak = 4,
                HighestStreak = 4,
                PointsClaimed = false,
                PointsClaimType = PointsClaimType.Yearly,
                TotalTimesClaimed = 4
            });

            await context.SaveChangesAsync();

            await context.TwitchSlotMachineStats.AddAsync(new TwitchSlotMachineStats
            {
                ChannelId = channel.Id,
                JackpotAmount = 1000,
                BookWins = 0,
                JackpotWins = 0,
                SmorcWins = 0,
                Tier1Wins = 0,
                Tier2Wins = 0,
                Tier3Wins = 0,
                TotalSpins = 0
            });

            await context.ChannelUserGambleStats.AddAsync(new ChannelUserGambleStats
            {
                BookWins = 0,
                ChannelId = channel.Id,
                ChannelUserId = channelUser.Id,
                JackpotWins = 0,
                PointsGambled = 0,
                PointsLost = 0,
                PointsWon = 0,
                SmorcWins = 0,
                Tier1Wins = 0,
                Tier2Wins = 0,
                Tier3Wins = 0,
                TotalSpins = 0
            });

            await context.SaveChangesAsync();

            await context.ChannelUserWatchtime.AddAsync(new ChannelUserWatchtime
            {
                ChannelId = channel.Id,
                ChannelUserId = channelUser.Id,
                MinutesWatchedThisStream = 0,
                MinutesWatchedThisWeek = 1,
                MinutesWatchedThisMonth = 2,
                MinutesWatchedThisYear = 3,
                MinutesInStream = 4
            });

            await context.ChannelUserWatchtime.AddAsync(new ChannelUserWatchtime
            {
                ChannelId = channel.Id,
                ChannelUserId = channelUser2.Id,
                MinutesWatchedThisStream = 0,
                MinutesWatchedThisWeek = 1,
                MinutesWatchedThisMonth = 2,
                MinutesWatchedThisYear = 3,
                MinutesInStream = 9
            });

            await context.ChannelRanks.AddAsync(new ChannelRank
            {
                BonusRankPointsEarned = 10000,
                ChannelId = channel.Id,
                RankMinutesRequired = 10,
                RankName = "tilly"
            });

            await context.SaveChangesAsync();

            await context.Blacklist.AddAsync(new Blacklist
            {
                ChannelId = channel.Id,
                Word = "seededbannedword",
                WordType = WordType.PermBanWord
            });

            await context.Blacklist.AddAsync(new Blacklist
            {
                ChannelId = channel.Id,
                Word = "seededfilteredword",
                WordType = WordType.TempBanWord
            });

            await context.Blacklist.AddAsync(new Blacklist
            {
                ChannelId = channel2.Id,
                Word = "seededbannedword",
                WordType = WordType.PermBanWord
            });

            await context.SaveChangesAsync();

            await context.DiscordLinkRequests.AddAsync(new DiscordLinkRequests
            {
                DiscordUserId = 1234567890, // Example Discord User ID
                TwitchLinkCode = 12345,
                TwitchUsername = Channel1User1TwitchUsername
            });

            await context.SaveChangesAsync();
        }
    }
}
