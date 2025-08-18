using BreganTwitchBot.Domain.Services;
using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Helpers
{
    [TestFixture]
    public class ConfigHelperTests
    {
        private PostgreSqlContainer _postgresContainer;
        private ServiceProvider _serviceProvider;
        private AppDbContext _dbContext;

        private ConfigHelperService _configHelperService;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .WithCleanUp(true) // Cleanup after test run
                .Build();

            await _postgresContainer.StartAsync();
        }

        [SetUp]
        public async Task Setup()
        {
            var services = new ServiceCollection();

            // Register DbContext with PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped
            );

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Resolve DbContext and set up database
            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _configHelperService = new ConfigHelperService(_serviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
            _serviceProvider.Dispose();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _postgresContainer.DisposeAsync();
        }

        [Test]
        public async Task UpdateDailyPointsStatus_UpdateDailyPointsStatusToTrue_NewValueIsSet()
        {
            await _configHelperService.UpdateDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, true);

            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            _dbContext.Entry(config).Reload();

            Assert.Multiple(() =>
            {
                Assert.That(config.DailyPointsCollectingAllowed, Is.True);
                Assert.That(config.LastDailyPointsAllowed, Is.EqualTo(DateTime.UtcNow).Within(30).Seconds);
            });
        }

        [Test]
        public async Task UpdateDailyPointsStatus_UpdateDailyPointsStatusToFalse_NewValueIsSet()
        {
            await _configHelperService.UpdateDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, false);
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            _dbContext.Entry(config).Reload();
            Assert.Multiple(() =>
            {
                Assert.That(config.DailyPointsCollectingAllowed, Is.False);
                Assert.That(config.LastDailyPointsAllowed, Is.Not.EqualTo(DateTime.UtcNow).Within(30).Seconds);
            });
        }

        [Test]
        public async Task GetDailyPointsStatus_GetDailyPointsStatus_ReturnsCorrectValues()
        {
            var status = _configHelperService.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.Multiple(() =>
            {
                Assert.That(status.DailyPointsAllowed, Is.EqualTo(config.DailyPointsCollectingAllowed));
                Assert.That(status.LastStreamDate, Is.EqualTo(config.LastStreamStartDate).Within(30).Seconds);
                Assert.That(status.LastDailyPointedAllowedDate, Is.EqualTo(config.LastDailyPointsAllowed).Within(30).Seconds);
                Assert.That(status.StreamHappenedThisWeek, Is.EqualTo(config.StreamHappenedThisWeek));
            });
        }

        [Test]
        public async Task UpdateStreamLiveStatus_UpdateStreamLiveStatusToTrue_ValuesAreSet()
        {
            await _configHelperService.UpdateStreamLiveStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, true);
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _dbContext.Entry(config).Reload();

            Assert.Multiple(() =>
            {
                Assert.That(config.StreamAnnounced, Is.True);
                Assert.That(config.LastStreamStartDate, Is.EqualTo(DateTime.UtcNow).Within(30).Seconds);
                Assert.That(config.StreamHappenedThisWeek, Is.True);
            });
        }

        [Test]
        public async Task UpdateStreamLiveStatus_UpdateStreamLiveStatusToFalse_ValuesAreSet()
        {
            await _configHelperService.UpdateStreamLiveStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, false);
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _dbContext.Entry(config).Reload();

            Assert.Multiple(() =>
            {
                Assert.That(config.StreamAnnounced, Is.False);
                Assert.That(config.LastStreamEndDate, Is.EqualTo(DateTime.UtcNow).Within(30).Seconds);
                Assert.That(config.StreamHappenedThisWeek, Is.True);
            });
        }

        [Test]
        public void GetDiscordConfig_NoDiscordGuildId_ReturnsNull()
        {
            var discordConfig = _configHelperService.GetDiscordConfig(0);
            Assert.That(discordConfig, Is.Null);
        }

        [Test]
        public void GetDiscordConfig_ValidDiscordGuildId_ReturnsConfig()
        {
            var discordConfig = _configHelperService.GetDiscordConfig(DatabaseSeedHelper.DiscordGuildId);
            Assert.That(discordConfig, Is.Not.Null);
        }

        [Test]
        public void GetDiscordConfig_InvalidBroadcasterId_ReturnsNull()
        {
            var discordConfig = _configHelperService.GetDiscordConfig("test");
            Assert.That(discordConfig, Is.Null);
        }

        [Test]
        public void GetDiscordConfig_ValidBroadcasterId_ReturnsConfig()
        {
            var discordConfig = _configHelperService.GetDiscordConfig(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            Assert.That(discordConfig, Is.Not.Null);
        }

        [Test]
        public void IsDiscordEnabled_DiscordNotEnabled_ReturnsFalse()
        {
            var isEnabled = _configHelperService.IsDiscordEnabled(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);
            Assert.That(isEnabled, Is.False);
        }

        [Test]
        public void IsDiscordEnabled_DiscordEnabled_ReturnsTrue()
        {
            var isEnabled = _configHelperService.IsDiscordEnabled(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            Assert.That(isEnabled, Is.True);
        }
    }
}
