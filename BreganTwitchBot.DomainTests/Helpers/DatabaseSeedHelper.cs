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
                BotTwitchChannelName = "CoolBotName",
                BroadcasterTwitchChannelId = "123",
                BroadcasterTwitchChannelName = "CoolStreamerName",
            };

            await context.Channels.AddAsync(channel);
            await context.SaveChangesAsync();

            await context.ChannelConfig.AddAsync(new ChannelConfig
            {
                ChannelId = channel.Id,
                DailyPointsCollectingAllowed = false,
                LastDailyPointsAllowed = DateTime.Now,
                StreamAnnounced = false,
                SubathonActive = false,
                ChannelCurrencyName = "CoolCurrencyName",
                CurrencyPointCap = 1000,
                StreamHappenedThisWeek = false,
                SubathonTime = TimeSpan.FromHours(1)
            });

            await context.SaveChangesAsync();

            var channelUser = new ChannelUser
            {
                AddedOn = DateTime.Now,
                CanUseOpenAi = false,
                DiscordUserId = 0,
                TwitchUserId = "456",
                TwitchUsername = "CoolUser",
            };

            await context.ChannelUsers.AddAsync(channelUser);
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

            await context.SaveChangesAsync();
        }
    }
}
