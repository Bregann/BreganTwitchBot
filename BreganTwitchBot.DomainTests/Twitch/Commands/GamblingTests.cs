using BreganTwitchBot.Domain.Services.Twitch.Commands.Gambling;
using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class GamblingTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private Mock<ITwitchHelperService> _twitchHelperService;

        private GamblingDataService _gamblingDataService;

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

            _twitchHelperService = new Mock<ITwitchHelperService>();
            _twitchHelperService.Setup(x => x.GetPointsName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("points");

            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _gamblingDataService = new GamblingDataService(_twitchHelperService.Object, _dbContext);
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
        public async Task HandleSpinCommand_StreamOffline_ReturnsCorrectMessage()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(false);
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!spin 100", "123", ["!spin", "100"]);

            var result = await _gamblingDataService.HandleSpinCommand(msgParams);
            Assert.That(result, Is.EqualTo("The streamer is offline so no gambling! No book book book for you"));
        }

        [Test]
        public void HandleSpinCommand_NoAmountSpecified_CorrectExceptionThrown()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(true);
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!spin", "123", ["!spin"]);

            var ex = Assert.ThrowsAsync<InvalidCommandException>(() => _gamblingDataService.HandleSpinCommand(msgParams));
            Assert.That(ex.Message, Is.EqualTo("You need to specify an amount to gamble! Usage: !spin <amount> or if you are an absolute mad lad do !spin all"));
        }

        [Test]
        public void HandleSpinCommand_InvalidAmount_CorrectExceptionThrown()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(true);
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!spin abc", "123", ["!spin", "abc"]);

            var ex = Assert.ThrowsAsync<InvalidCommandException>(() => _gamblingDataService.HandleSpinCommand(msgParams));
            Assert.That(ex.Message, Is.EqualTo("You need to specify a valid amount to gamble! Usage: !spin <amount> or if you are an absolute mad lad do !spin all"));
        }

        [Test]
        public void HandleSpinCommand_NotEnoughPoints_CorrectExceptionThrown()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(true);
            _twitchHelperService.Setup(x => x.GetPointsForUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(100);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!spin 200", "123", ["!spin", "200"]);
            var ex = Assert.ThrowsAsync<InvalidCommandException>(() => _gamblingDataService.HandleSpinCommand(msgParams));

            Assert.That(ex.Message, Is.EqualTo("You do not have enough points to gamble that amount! You have 100 points"));
        }

        [Test]
        public void HandleSpinCommand_TooLittlePointsGambled_CorrectExceptionThrown()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(true);
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!spin 50", "123", ["!spin", "50"]);

            var ex = Assert.ThrowsAsync<InvalidCommandException>(() => _gamblingDataService.HandleSpinCommand(msgParams));
            Assert.That(ex.Message, Is.EqualTo("You have to gamble at least 100 points! Usage: !spin <amount> or if you are an absolute mad lad do !spin all"));
        }

        [Test]
        public async Task HandleSpinCommand_AllPointsGambled_ReturnsCorrectMessage()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(true);
            _twitchHelperService.Setup(x => x.GetPointsForUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(200);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!spin all", "123", ["!spin", "all"]);

            var result = await _gamblingDataService.HandleSpinCommand(msgParams);
            var gambleStats = await _dbContext.TwitchSlotMachineStats.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);
            var userGambleStats = await _dbContext.ChannelUserGambleStats.FirstAsync(x => x.ChannelUser.TwitchUserId == msgParams.ChatterChannelId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(gambleStats.TotalSpins, Is.EqualTo(1));
                Assert.That(userGambleStats.PointsGambled, Is.EqualTo(300));
                Assert.That(userGambleStats.TotalSpins, Is.EqualTo(23));
            });
        }

        [Test]
        public async Task HandleSpinCommand_ValidAmount_ReturnsCorrectMessage()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(true);
            _twitchHelperService.Setup(x => x.GetPointsForUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(200);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!spin 200", "123", ["!spin", "200"]);
            var result = await _gamblingDataService.HandleSpinCommand(msgParams);
            var gambleStats = await _dbContext.TwitchSlotMachineStats.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);
            var userGambleStats = await _dbContext.ChannelUserGambleStats.FirstAsync(x => x.ChannelUser.TwitchUserId == msgParams.ChatterChannelId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(gambleStats.TotalSpins, Is.EqualTo(1));
                Assert.That(userGambleStats.PointsGambled, Is.EqualTo(300));
                Assert.That(userGambleStats.TotalSpins, Is.EqualTo(23));
            });

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetJackpotAmount_ReturnsCorrectMessage()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!jackpot", "123", ["!jackpot"]);
            var result = await _gamblingDataService.GetJackpotAmount(msgParams);
            Assert.That(result, Is.Not.Null);
        }
    }
}
