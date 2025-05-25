using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Leaderboards;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class LeaderboardsCommandsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private LeaderboardsDataService _leaderboardsDataService;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("testdb_leaderboards")
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

            _leaderboardsDataService = new LeaderboardsDataService(_dbContext);
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
        [TestCase(LeaderboardType.Points, "#1 - supermoduser - 5,555 | #2 - cooluser - 100 | #3 - cooluser2 - 73")]
        [TestCase(LeaderboardType.AllTimeHours, "#1 - cooluser2 - 9 minutes | #2 - cooluser - 4 minutes")]
        [TestCase(LeaderboardType.StreamHours, "#1 - cooluser2 - 10 minutes | #2 - cooluser - 0 minutes")]
        [TestCase(LeaderboardType.WeeklyHours, "#1 - cooluser2 - 5 minutes | #2 - cooluser - 1 minutes")]
        [TestCase(LeaderboardType.MonthlyHours, "#1 - cooluser2 - 7 minutes | #2 - cooluser - 2 minutes")]
        [TestCase(LeaderboardType.PointsWon, "#1 - cooluser2 - 5,555 | #2 - cooluser - 2,111")]
        [TestCase(LeaderboardType.PointsLost, "#1 - cooluser - 333,333,333 | #2 - cooluser2 - 44")]
        [TestCase(LeaderboardType.PointsGambled, "#1 - cooluser - 4,444,444 | #2 - cooluser2 - 0")]
        [TestCase(LeaderboardType.TotalSpins, "#1 - cooluser2 - 9,999 | #2 - cooluser - 22")]
        [TestCase(LeaderboardType.CurrentDailyStreak, "#1 - cooluser2 - 2 | #2 - cooluser - 1")]
        [TestCase(LeaderboardType.HighestDailyStreak, "#1 - cooluser2 - 2 | #2 - cooluser - 1")]
        [TestCase(LeaderboardType.CurrentWeeklyStreak, "#1 - cooluser2 - 3 | #2 - cooluser - 2")]
        [TestCase(LeaderboardType.HighestWeeklyStreak, "#1 - cooluser2 - 3 | #2 - cooluser - 2")]
        [TestCase(LeaderboardType.CurrentMonthlyStreak, "#1 - cooluser2 - 4 | #2 - cooluser - 3")]
        [TestCase(LeaderboardType.HighestMonthlyStreak, "#1 - cooluser2 - 4 | #2 - cooluser - 3")]
        [TestCase(LeaderboardType.CurrentYearlyStreak, "#1 - cooluser2 - 5 | #2 - cooluser - 4")]
        [TestCase(LeaderboardType.HighestYearlyStreak, "#1 - cooluser2 - 5 | #2 - cooluser - 4")]
        [TestCase(LeaderboardType.BossesDone, "#1 - cooluser - 10 | #2 - cooluser2 - 3")]
        [TestCase(LeaderboardType.BossesPointsWon, "#1 - cooluser - 55,555 | #2 - cooluser2 - 7,373")]
        public async Task HandleLeaderboardCommand_ShouldReturnCorrectLeaderboardString(LeaderboardType leaderboardType, string expectedResponse)
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams(
                message: "!leaderboard",
                messageId: "leaderboard-test-msg-id",
                messageParts: new string[] { "!leaderboard" },
                broadcasterChannelId: DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                broadcasterChannelName: DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName
            );

            var actualResponse = await _leaderboardsDataService.HandleLeaderboardCommand(msgParams, leaderboardType);

            Assert.That(actualResponse, Is.EqualTo(expectedResponse), $"Assertion failed for LeaderboardType: {leaderboardType}");
        }
    }
}
