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
using Hangfire;
using BreganTwitchBot.Domain.Enums;

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

            _configHelper = new Mock<IConfigHelper>();
            _twitchHelperService = new Mock<ITwitchHelperService>();

            _twitchHelperService
                .Setup(x => x.SendTwitchMessageToChannel(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()
                ))
                .Returns(Task.CompletedTask);

            var mockRecurringJobManager = new Mock<IRecurringJobManager>();

            _dailyPointsDataService = new DailyPointsDataService(_dbContext, _configHelper.Object, _twitchHelperService.Object, mockRecurringJobManager.Object);
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

            _configHelper.Verify(x => x.UpdateDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, true), Times.Once);

            var dailyPoints = await _dbContext.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.PointsClaimType == Domain.Enums.PointsClaimType.Daily).FirstAsync();
            Assert.That(dailyPoints.CurrentStreak, Is.EqualTo(0));
        }

        [Test]
        public async Task AllowDailyPointsCollecting_ShouldNotResetStreaksWhenStreamToday_StreaksNotReset()
        {
            _configHelper.Setup(x => x.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Returns((false, DateTime.UtcNow, DateTime.UtcNow));

            await _dailyPointsDataService.AllowDailyPointsCollecting(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _configHelper.Verify(x => x.UpdateDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, true), Times.Once);

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                It.IsAny<string>(),
                It.Is<string>(msg => msg.Contains("Top 5 lost streaks")),
                It.IsAny<string?>()),
                Times.Never);
        }

        [Test]
        [TestCase(PointsClaimType.Daily)]
        [TestCase(PointsClaimType.Weekly)]
        [TestCase(PointsClaimType.Monthly)]
        [TestCase(PointsClaimType.Yearly)]
        public async Task HandlePointsClaimed_AttemptToClaimWhenPointsDisabled_ShouldNotAllowClaimingPoints(PointsClaimType pointsClaimType)
        {
            _configHelper.Setup(x => x.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Returns((false, DateTime.UtcNow, DateTime.UtcNow));
            _twitchHelperService.Setup(x => x.GetPointsName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .ReturnsAsync(DatabaseSeedHelper.Channel1ChannelCurrencyName);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!daily", "123", new string[] { "!daily" });
            var response = await _dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);

            Assert.That(response, Is.EqualTo($"Daily {DatabaseSeedHelper.Channel1ChannelCurrencyName} collection is not allowed at the moment! The stream has to be online for 30 minutes (ish) before claiming"));
        }

        [Test]
        [TestCase(PointsClaimType.Daily)]
        [TestCase(PointsClaimType.Weekly)]
        [TestCase(PointsClaimType.Monthly)]
        [TestCase(PointsClaimType.Yearly)]
        public async Task HandlePointsClaimed_AttemptToClaimWithInvalidUsername_CorrectMessageReturned(PointsClaimType pointsClaimType)
        {
            _configHelper.Setup(x => x.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Returns((true, DateTime.UtcNow, DateTime.UtcNow));

            _twitchHelperService.Setup(x => x.GetPointsName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .ReturnsAsync(DatabaseSeedHelper.Channel1ChannelCurrencyName);

            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!daily", "123", new string[] { "!daily" }, chatterChannelId: "2545555", chatterChannelName: "invalid");
            var response = await _dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);
            Assert.That(response, Is.EqualTo($"Oops! Looks like you're not quite ready to claim your {DatabaseSeedHelper.Channel1ChannelCurrencyName} just yet. Give it a moment and try again!"));
        }

        [Test]
        [TestCase(PointsClaimType.Daily)]
        [TestCase(PointsClaimType.Weekly)]
        [TestCase(PointsClaimType.Monthly)]
        [TestCase(PointsClaimType.Yearly)]
        public async Task HandlePointsClaimed_AttemptToClaimWithPointsAlreadyClaimed_CorrectMessageReturned(PointsClaimType pointsClaimType)
        {
            _configHelper.Setup(x => x.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Returns((true, DateTime.UtcNow, DateTime.UtcNow));

            _twitchHelperService.Setup(x => x.GetPointsName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .ReturnsAsync(DatabaseSeedHelper.Channel1ChannelCurrencyName);

            _dbContext.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.PointsClaimType == pointsClaimType)
                .First().PointsClaimed = true;

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!daily", "123", new string[] { "!daily" });
            var response = await _dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);

            Assert.That(response, Is.EqualTo($"You silly sausage! You have claimed your {pointsClaimType.ToString().ToLower()} {DatabaseSeedHelper.Channel1ChannelCurrencyName}!"));
        }

        [Test]
        [TestCase(PointsClaimType.Daily)]
        [TestCase(PointsClaimType.Weekly)]
        [TestCase(PointsClaimType.Monthly)]
        [TestCase(PointsClaimType.Yearly)]

    }
}
