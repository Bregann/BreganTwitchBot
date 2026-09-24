using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Services.Api;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Api
{
    [TestFixture]
    public class MeDataServiceTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private MeDataService _meDataService;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .WithCleanUp(true)
                .Build();

            await _postgresContainer.StartAsync();
        }

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseLazyLoadingProxies()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            _dbContext = new AppDbContext(options);
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _meDataService = new MeDataService(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _postgresContainer.DisposeAsync();
        }

        [Test]
        public async Task GetMyStats_ReturnsTheChannelsTheUserIsKnownIn()
        {
            var result = await _meDataService.GetMyStatsAsync(DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.TwitchUsername, Is.EqualTo(DatabaseSeedHelper.Channel1User1TwitchUsername));
                Assert.That(result.Channels, Is.Not.Empty);
            });
        }

        [Test]
        public async Task GetMyStats_IncludesThePointsUnderTheChannelCurrencyName()
        {
            var result = await _meDataService.GetMyStatsAsync(DatabaseSeedHelper.Channel1User1TwitchUserId);

            var channel = result!.Channels.First(x => x.BroadcasterChannelName == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.That(channel.PointsName, Is.EqualTo(DatabaseSeedHelper.Channel1ChannelCurrencyName));
        }

        [Test]
        public async Task GetMyStats_UnknownUser_ReturnsNull()
        {
            // signed in but never seen by the bot
            var result = await _meDataService.GetMyStatsAsync("nobody-the-bot-has-seen");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetMyStats_OnlyReturnsChannelsTheUserIsActuallyIn()
        {
            var result = await _meDataService.GetMyStatsAsync(DatabaseSeedHelper.Channel2User1TwitchUserId);

            Assert.That(
                result!.Channels.Select(x => x.BroadcasterChannelName),
                Does.Not.Contain(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName));
        }

        [Test]
        public async Task GetMyStats_ReportsRankProgress()
        {
            var result = await _meDataService.GetMyStatsAsync(DatabaseSeedHelper.Channel1User1TwitchUserId);
            var channel = result!.Channels.First(x => x.BroadcasterChannelName == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            // the seeded channel has ranks, so there should be a next one to aim at
            Assert.That(channel.NextRank, Is.Not.Null);
            Assert.That(channel.MinutesUntilNextRank, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public async Task GetMyStats_ChannelsAreOrderedByWatchtime()
        {
            var result = await _meDataService.GetMyStatsAsync(DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.That(result!.Channels.Select(x => x.MinutesInStream), Is.Ordered.Descending);
        }

        [Test]
        public async Task GetMySettings_WithNoDiscordStats_ReturnsEmpty()
        {
            var result = await _meDataService.GetMySettingsAsync(DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.That(result.Channels, Is.Empty);
        }

        [Test]
        public async Task UpdateMySetting_ChangesTheirOwnPreference()
        {
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _dbContext.DiscordUserStats.Add(new Domain.Database.Models.DiscordUserStats
            {
                ChannelUserId = user.Id,
                ChannelId = channel.Id,
                DiscordLevel = 1,
                DiscordXp = 0,
                DiscordLevelUpNotifsEnabled = true,
                PrestigeLevel = 0
            });
            await _dbContext.SaveChangesAsync();

            await _meDataService.UpdateMySettingAsync(DatabaseSeedHelper.Channel1User1TwitchUserId, new Domain.DTOs.Api.UpdateMySettingRequest
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                DiscordLevelUpNotifsEnabled = false
            });

            var stats = await _dbContext.DiscordUserStats.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);
            Assert.That(stats.DiscordLevelUpNotifsEnabled, Is.False);
        }

        [Test]
        public void UpdateMySetting_ForAChannelTheyAreNotIn_IsRejected()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _meDataService.UpdateMySettingAsync(DatabaseSeedHelper.Channel1User1TwitchUserId, new Domain.DTOs.Api.UpdateMySettingRequest
                {
                    BroadcasterChannelName = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName,
                    DiscordLevelUpNotifsEnabled = false
                }));
        }

        [Test]
        public void UpdateMySetting_ForAnUnknownUser_IsRejected()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _meDataService.UpdateMySettingAsync("nobody", new Domain.DTOs.Api.UpdateMySettingRequest
                {
                    BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                    DiscordLevelUpNotifsEnabled = false
                }));
        }
    }
}
