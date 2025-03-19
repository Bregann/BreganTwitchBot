using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;

namespace BreganTwitchBot.DomainTests.Helpers
{
    public class DatabaseSeedHelper
    {
        public static async Task SeedDatabase(AppDbContext context)
        {
            var channel = new Channel
            {
                BotTwitchChannelOAuthToken = "",
                BroadcasterTwitchChannelOAuthToken = "",
                BotTwitchChannelId = "",
                BotTwitchChannelRefreshToken = "",
                BroadcasterTwitchChannelRefreshToken = "",
                BotTwitchChannelName = "coolbotname",
                BroadcasterTwitchChannelId = "123",
                BroadcasterTwitchChannelName = "coolstreamername",
            };

            var channel2 = new Channel
            {
                BotTwitchChannelOAuthToken = "",
                BroadcasterTwitchChannelOAuthToken = "",
                BotTwitchChannelId = "",
                BotTwitchChannelRefreshToken = "",
                BroadcasterTwitchChannelRefreshToken = "",
                BotTwitchChannelName = "coolbotname2",
                BroadcasterTwitchChannelId = "12345",
                BroadcasterTwitchChannelName = "coolstreamername2",
            };

            await context.Channels.AddAsync(channel);
            await context.Channels.AddAsync(channel2);
            await context.SaveChangesAsync();

            await context.ChannelConfig.AddAsync(new ChannelConfig
            {
                ChannelId = channel.Id,
                DailyPointsCollectingAllowed = false,
                LastDailyPointsAllowed = DateTime.UtcNow,
                StreamAnnounced = false,
                SubathonActive = false,
                ChannelCurrencyName = "CoolCurrencyName",
                CurrencyPointCap = 1000,
                StreamHappenedThisWeek = false,
                SubathonTime = TimeSpan.FromHours(1)
            });

            await context.ChannelConfig.AddAsync(new ChannelConfig
            {
                ChannelId = channel2.Id,
                DailyPointsCollectingAllowed = false,
                LastDailyPointsAllowed = DateTime.UtcNow,
                StreamAnnounced = false,
                SubathonActive = false,
                ChannelCurrencyName = "CoolCurrencyName2",
                CurrencyPointCap = 2000,
                StreamHappenedThisWeek = false,
                SubathonTime = TimeSpan.FromHours(1)
            });

            await context.SaveChangesAsync();

            var channelUser = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = "456",
                TwitchUsername = "cooluser",
            };

            var channelUser2 = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = "789",
                TwitchUsername = "cooluser2",
            };

            var channelUser3 = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = "999",
                TwitchUsername = "cooluser3",
            };

            var superModUser = new ChannelUser
            {
                AddedOn = DateTime.UtcNow,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = "1111",
                TwitchUsername = "supermoduser",
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
        }
    }
}
