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
                LastStreamEndDate = DateTime.UtcNow.AddDays(-1)
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
                LastStreamEndDate = DateTime.UtcNow.AddDays(-1)
            });

            await context.SaveChangesAsync();

            var channelUser = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel1User1TwitchUserId,
                TwitchUsername = Channel1User1TwitchUsername,
            };

            var channelUser2 = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel1User2TwitchUserId,
                TwitchUsername = Channel1User2TwitchUsername,
            };

            var channelUser3 = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel2User1TwitchUserId,
                TwitchUsername = Channel2User1TwitchUsername,
            };

            var superModUser = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = Channel1SuperModUserTwitchUserId,
                TwitchUsername = Channel1SuperModUserTwitchUsername,
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
        }
    }
}
