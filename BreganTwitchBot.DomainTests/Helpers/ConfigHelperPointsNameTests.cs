using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Services;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Helpers
{
    [TestFixture]
    public class ConfigHelperPointsNameTests
    {
        private PostgreSqlContainer _postgresContainer;
        private ServiceProvider _serviceProvider;
        private AppDbContext _dbContext;

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
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseLazyLoadingProxies().UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped);

            _serviceProvider = services.BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            // the guild has to be linked before the config helper loads its cache
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            config.DiscordGuildId = DatabaseSeedHelper.DiscordGuildId;
            await _dbContext.SaveChangesAsync();
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

        private ConfigHelperService CreateService() => new(_serviceProvider);

        [Test]
        public void GetPointsName_ReturnsTheChannelsCurrencyName()
        {
            var service = CreateService();

            Assert.That(
                service.GetPointsName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId),
                Is.EqualTo(DatabaseSeedHelper.Channel1ChannelCurrencyName));
        }

        [Test]
        public void GetPointsName_IsPerChannel()
        {
            var service = CreateService();

            Assert.That(
                service.GetPointsName(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId),
                Is.EqualTo(DatabaseSeedHelper.Channel2ChannelCurrencyName));
        }

        [Test]
        public void GetPointsName_UnknownChannel_FallsBackRatherThanThrowing()
        {
            var service = CreateService();

            Assert.That(service.GetPointsName("notachannel"), Is.EqualTo("points"));
        }

        [Test]
        public void GetPointsNameForGuild_ReturnsTheLinkedChannelsCurrencyName()
        {
            var service = CreateService();

            Assert.That(
                service.GetPointsNameForGuild(DatabaseSeedHelper.DiscordGuildId),
                Is.EqualTo(DatabaseSeedHelper.Channel1ChannelCurrencyName));
        }

        [Test]
        public void GetPointsNameForGuild_UnknownGuild_ReturnsNull()
        {
            var service = CreateService();

            Assert.That(service.GetPointsNameForGuild(99999999), Is.Null);
        }
    }
}
