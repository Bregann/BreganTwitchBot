using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async Task UpdateDailyPointsStatus_UpdateDailyPointsStatus_NewValueIsSet()
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
        public async Task GetDailyPointsStatus_GetDailyPointsStatus_ReturnsCorrectValues()
        {
            var (DailyPointsAllowed, LastStreamDate, LastDailyPointedAllowedDate) = _configHelperService.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            
            Assert.Multiple(() =>
            {
                Assert.That(DailyPointsAllowed, Is.EqualTo(config.DailyPointsCollectingAllowed));
                Assert.That(LastStreamDate, Is.EqualTo(config.LastStreamEndDate).Within(30).Seconds);
                Assert.That(LastDailyPointedAllowedDate, Is.EqualTo(config.LastDailyPointsAllowed).Within(30).Seconds);
            });
        }
    }
}
