using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using TwitchLib.Api;
using BreganTwitchBot.DomainTests.Helpers;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.DailyPoints;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Data.Database.Models;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class DailyPointsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<IConfigHelper> _configHelper;
        private Mock<ITwitchHelperService> _twitchHelperService;

        private DailyPointsDataService _dailyPointsDataService;

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
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseLazyLoadingProxies()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            _dbContext = new AppDbContext(options);
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _twitchHelperService
                .Setup(x => x.SendTwitchMessageToChannel(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()
                ))
                .Returns(Task.CompletedTask);

            _configHelper = new Mock<IConfigHelper>();
            _twitchHelperService = new Mock<ITwitchHelperService>();

            _dailyPointsDataService = new DailyPointsDataService(_dbContext, _configHelper.Object, _twitchHelperService.Object);
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
        public async Task AllowDailyPointsCollecting_ShouldResetStreaksWhenNoStreamToday_StreaksGetReset()
        {
            await _dailyPointsDataService.AllowDailyPointsCollecting(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _configHelper.Verify(x => x.UpdateDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, true), Times.Once);

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                It.IsAny<string>(),
                It.Is<string>(msg => msg.Contains("Top 5 lost streaks")),
                It.IsAny<string?>()),
                Times.Once);

            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var dailyPoints = await _dbContext.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId).FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(config.DailyPointsCollectingAllowed, Is.True);
                Assert.That(dailyPoints.CurrentDailyStreak, Is.EqualTo(0));
            });
        }

        [Test]
        public async Task AllowDailyPointsCollecting_ShouldNotResetStreaksWhenStreamToday_StreaksNotReset()
        {
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            config.LastStreamStartDate = DateTime.UtcNow;
            config.LastStreamEndDate = DateTime.UtcNow.AddHours(1);
            await _dbContext.SaveChangesAsync();

            await _dailyPointsDataService.AllowDailyPointsCollecting(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _configHelper.Verify(x => x.UpdateDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, true), Times.Once);
            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                It.IsAny<string>(),
                It.Is<string>(msg => msg.Contains("Top 5 lost streaks")),
                It.IsAny<string?>()),
                Times.Never);

            var updatedConfig = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var dailyPoints = await _dbContext.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId).FirstAsync();
            
            Assert.Multiple(() =>
            {
                Assert.That(updatedConfig.DailyPointsCollectingAllowed, Is.True);
                Assert.That(dailyPoints.CurrentDailyStreak, Is.EqualTo(1));
            });
        }
    }
}
