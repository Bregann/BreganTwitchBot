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
using BreganTwitchBot.Domain.Exceptions;

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

            _twitchHelperService.Setup(x => x.GetPointsName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .ReturnsAsync(DatabaseSeedHelper.Channel1ChannelCurrencyName);

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

            _dbContext.TwitchDailyPoints.Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.PointsClaimType == pointsClaimType)
                .First().PointsClaimed = true;

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!daily", "123", new string[] { "!daily" });
            var response = await _dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);

            Assert.That(response, Is.EqualTo($"You silly sausage! You have claimed your {pointsClaimType.ToString().ToLower()} {DatabaseSeedHelper.Channel1ChannelCurrencyName}!"));
        }

        [Test]
        [TestCase(PointsClaimType.Daily, 2, 500)]
        [TestCase(PointsClaimType.Weekly, 3, 3000)]
        [TestCase(PointsClaimType.Monthly, 4, 16000)]
        [TestCase(PointsClaimType.Yearly, 5, 1)]
        public async Task HandlePointsClaimed_AttemptToClaimNormallyWithNewHighestStreak_CorrectMessageReturnedAndDataSet(PointsClaimType pointsClaimType, int expectedStreakNumber, int expectedPoints)
        {
            _configHelper.Setup(x => x.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Returns((true, DateTime.UtcNow, DateTime.UtcNow));

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!daily", "123", new string[] { "!daily" });
            var response = await _dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);

            var dailyPoints = await _dbContext.TwitchDailyPoints
                .Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.PointsClaimType == pointsClaimType)
                .FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(dailyPoints.PointsClaimed, Is.True);
                Assert.That(dailyPoints.CurrentStreak, Is.EqualTo(expectedStreakNumber));
                Assert.That(dailyPoints.HighestStreak, Is.EqualTo(expectedStreakNumber));
                Assert.That(dailyPoints.TotalTimesClaimed, Is.EqualTo(expectedStreakNumber));
            });

            //yearly is always a random amount so its hard to check that the specific points are correct
            if (pointsClaimType == PointsClaimType.Yearly)
            {
                Assert.That(dailyPoints.TotalPointsClaimed, Is.GreaterThanOrEqualTo(expectedPoints));
            }
            else
            {
                Assert.That(dailyPoints.TotalPointsClaimed, Is.EqualTo(expectedPoints));
                Assert.That(response, Is.EqualTo($"You have claimed your {pointsClaimType.ToString().ToLower()} {DatabaseSeedHelper.Channel1ChannelCurrencyName} for {expectedPoints:N0} points! You are on a {expectedStreakNumber} day streak!"));
            }
        }

        [Test]
        [TestCase(PointsClaimType.Daily)]
        [TestCase(PointsClaimType.Weekly)]
        [TestCase(PointsClaimType.Monthly)]
        [TestCase(PointsClaimType.Yearly)]
        public async Task HandlePointsClaimed_AttemptToClaimNormallyWithHighestStreakAlreadyHigher_HighestStreakIsNotUpdated(PointsClaimType pointsClaimType)
        {
            _configHelper.Setup(x => x.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Returns((true, DateTime.UtcNow, DateTime.UtcNow));

            var dailyPoints = await _dbContext.TwitchDailyPoints
                .Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.PointsClaimType == pointsClaimType)
                .FirstAsync();

            dailyPoints.HighestStreak = 10;
            await _dbContext.SaveChangesAsync();

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!daily", "123", new string[] { "!daily" });
            var response = await _dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);

            Assert.Multiple(() =>
            {
                Assert.That(dailyPoints.HighestStreak, Is.EqualTo(10));
                Assert.That(dailyPoints.PointsClaimed, Is.EqualTo(true));
            });
        }

        [Test]
        [TestCase(PointsClaimType.Daily, 99)]
        [TestCase(PointsClaimType.Daily, 49)]
        [TestCase(PointsClaimType.Daily, 24)]
        [TestCase(PointsClaimType.Daily, 9)]
        [TestCase(PointsClaimType.Weekly, 9)]
        [TestCase(PointsClaimType.Monthly, 5)]
        [TestCase(PointsClaimType.Yearly, 1)]
        public async Task HandlePointsClaimed_ClaimPointsWhenAtMilestone_BonusMessageSent(PointsClaimType pointsClaimType, int currentTotalClaims)
        {
            _configHelper.Setup(x => x.GetDailyPointsStatus(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                            .Returns((true, DateTime.UtcNow, DateTime.UtcNow));

            var dailyPoints = await _dbContext.TwitchDailyPoints
                .Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.PointsClaimType == pointsClaimType)
                .FirstAsync();

            dailyPoints.TotalTimesClaimed = currentTotalClaims;
            await _dbContext.SaveChangesAsync();

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!daily", "123", new string[] { "!daily" });
            var response = await _dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);

            Assert.That(response, Does.Contain($"As this is your {currentTotalClaims + 1}{(pointsClaimType == PointsClaimType.Yearly ? "nd" : "th")} time claiming your {pointsClaimType.ToString().ToLower()} {DatabaseSeedHelper.Channel1ChannelCurrencyName}, you have been gifted an extra"));
        }

        [Test]
        [TestCase(PointsClaimType.Daily)]
        [TestCase(PointsClaimType.Weekly)]
        [TestCase(PointsClaimType.Monthly)]
        [TestCase(PointsClaimType.Yearly)]
        public async Task HandleStreakCheckCommand_CheckWithInvalidUsername_CorrectMessageReturned(PointsClaimType pointsClaimType)
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!streak", "123", new string[] { "!streak" }, chatterChannelId: "2545555", chatterChannelName: "invalid");
            var response = await _dailyPointsDataService.HandleStreakCheckCommand(msgParams, pointsClaimType);

            Assert.That(response, Is.EqualTo("You haven't started a streak yet!"));
        }

        [Test]
        [TestCase(PointsClaimType.Daily)]
        [TestCase(PointsClaimType.Weekly)]
        [TestCase(PointsClaimType.Monthly)]
        [TestCase(PointsClaimType.Yearly)]
        public async Task HandleStreakCheckCommand_CheckOtherUserInvalidUsername_CorrectExceptionThrown(PointsClaimType pointsClaimType)
        {
            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!streak invalid", "123", new string[] { "!daily", "invalid" });

            Assert.ThrowsAsync<TwitchUserNotFoundException>(() => _dailyPointsDataService.HandleStreakCheckCommand(msgParams, pointsClaimType));
        }

        [Test]
        [TestCase(PointsClaimType.Daily, 1)]
        [TestCase(PointsClaimType.Weekly, 2)]
        [TestCase(PointsClaimType.Monthly, 3)]
        [TestCase(PointsClaimType.Yearly, 4)]
        public async Task HandleStreakCheckCommand_CheckWithStreak_CorrectMessageReturned(PointsClaimType pointsClaimType, int currentStreak)
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!streak", "123", new string[] { "!streak" });
            var response = await _dailyPointsDataService.HandleStreakCheckCommand(msgParams, pointsClaimType);
            Assert.That(response, Is.EqualTo($"You are on a {currentStreak} {pointsClaimType.ToString().ToLower().TrimEnd(['l', 'y'])} streak!"));
        }

        [Test]
        [TestCase(PointsClaimType.Daily, 2)]
        [TestCase(PointsClaimType.Weekly, 3)]
        [TestCase(PointsClaimType.Monthly, 4)]
        [TestCase(PointsClaimType.Yearly, 5)]
        public async Task HandleStreakCheckCommand_CheckWithStreakOtherUser_CorrectMessageReturned(PointsClaimType pointsClaimType, int currentStreak)
        {
            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(DatabaseSeedHelper.Channel1User2TwitchUsername))
                .ReturnsAsync(DatabaseSeedHelper.Channel1User2TwitchUserId);

            var msgParams = MessageParamsHelper.CreateChatMessageParams($"!streak {DatabaseSeedHelper.Channel1User2TwitchUsername}", "123", new string[] { "!streak", DatabaseSeedHelper.Channel1User2TwitchUsername });
            var response = await _dailyPointsDataService.HandleStreakCheckCommand(msgParams, pointsClaimType);

            Assert.That(response, Is.EqualTo($"{DatabaseSeedHelper.Channel1User2TwitchUsername} is on a {currentStreak} {pointsClaimType.ToString().ToLower().TrimEnd(['l', 'y'])} streak!"));
        }
    }
}
