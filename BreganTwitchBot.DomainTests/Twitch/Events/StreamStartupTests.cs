using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Twitch.Events;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BreganTwitchBot.DomainTests.Twitch.Events
{
    /// <summary>
    /// The bot restarting while a stream it already knew about is live used to be handled as the
    /// stream going live again: stream minutes wiped, @everyone pinged and daily points opened early.
    /// </summary>
    [TestFixture]
    public class StreamStartupTests
    {
        private const string BroadcasterId = "123";
        private const string BroadcasterName = "coolstreamername";

        private Mock<ITwitchHelperService> _twitchHelperService;
        private Mock<IConfigHelperService> _configHelperService;
        private Mock<IDailyPointsDataService> _dailyPointsDataService;
        private Mock<IHoursDataService> _hoursDataService;
        private ServiceProvider _serviceProvider;

        private TwitchEventHandlerService _eventHandlerService;

        [SetUp]
        public void Setup()
        {
            // the boss countdown and daily points are scheduled through hangfire
            JobStorage.Current = new MemoryStorage();

            _twitchHelperService = new Mock<ITwitchHelperService>();
            _configHelperService = new Mock<IConfigHelperService>();
            _dailyPointsDataService = new Mock<IDailyPointsDataService>();
            _hoursDataService = new Mock<IHoursDataService>();

            var services = new ServiceCollection();
            services.AddSingleton(_dailyPointsDataService.Object);
            services.AddSingleton(_hoursDataService.Object);
            _serviceProvider = services.BuildServiceProvider();

            _eventHandlerService = new TwitchEventHandlerService(
                _twitchHelperService.Object,
                new Mock<ITwitchApiInteractionService>().Object,
                new Mock<ITwitchApiConnection>().Object,
                _configHelperService.Object,
                new Mock<IDiscordHelperService>().Object,
                _serviceProvider,
                new Mock<IStreamStatsService>().Object);
        }

        [TearDown]
        public void TearDown()
        {
            _serviceProvider.Dispose();
        }

        /// <summary>
        /// What the bot had stored before restarting
        /// </summary>
        private void Stored(bool live, DateTime lastStreamStart, bool dailyPointsAllowed)
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(BroadcasterId)).ReturnsAsync(live);
            _configHelperService.Setup(x => x.GetDailyPointsStatus(BroadcasterId))
                .Returns((dailyPointsAllowed, lastStreamStart, lastStreamStart, true));
        }

        private void VerifyWentLive(Times times)
        {
            _configHelperService.Verify(x => x.UpdateStreamLiveStatus(BroadcasterId, true), times);
        }

        private static int ScheduledJobs() => (int)JobStorage.Current.GetMonitoringApi().ScheduledCount();

        [Test]
        public async Task RestartMidStream_DoesNotGoLiveAgain()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-5);
            Stored(live: true, lastStreamStart: startedAt.AddSeconds(10), dailyPointsAllowed: true);

            await _eventHandlerService.HandleStreamLiveOnStartup(BroadcasterId, BroadcasterName, startedAt);

            VerifyWentLive(Times.Never());
            _hoursDataService.Verify(x => x.ResetStreamMinutesForBroadcaster(It.IsAny<string>()), Times.Never());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(It.IsAny<string>()), Times.Never());
            Assert.That(ScheduledJobs(), Is.Zero, "no second boss countdown or daily points job");
        }

        [Test]
        public async Task RestartMidStream_BeforeDailyPointsOpen_ReschedulesThemRatherThanOpeningEarly()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-5);
            Stored(live: true, lastStreamStart: startedAt.AddSeconds(10), dailyPointsAllowed: false);

            await _eventHandlerService.HandleStreamLiveOnStartup(BroadcasterId, BroadcasterName, startedAt);

            VerifyWentLive(Times.Never());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(It.IsAny<string>()), Times.Never());
            Assert.That(ScheduledJobs(), Is.EqualTo(1));
        }

        [Test]
        public async Task RestartMidStream_AfterDailyPointsShouldHaveOpened_OpensThemNow()
        {
            // e.g. the bot was down when the scheduled job was due
            var startedAt = DateTime.UtcNow.AddMinutes(-45);
            Stored(live: true, lastStreamStart: startedAt.AddSeconds(10), dailyPointsAllowed: false);

            await _eventHandlerService.HandleStreamLiveOnStartup(BroadcasterId, BroadcasterName, startedAt);

            VerifyWentLive(Times.Never());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task StreamTheBotDidNotKnowWasLive_GoesLive()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-5);
            Stored(live: false, lastStreamStart: DateTime.UtcNow.AddDays(-1), dailyPointsAllowed: false);

            await _eventHandlerService.HandleStreamLiveOnStartup(BroadcasterId, BroadcasterName, startedAt);

            VerifyWentLive(Times.Once());
            _hoursDataService.Verify(x => x.ResetStreamMinutesForBroadcaster(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task NewStreamWhileTheBotMissedTheLastOneEnding_GoesLive()
        {
            // still marked live from yesterday, as the bot was down when that stream ended
            var startedAt = DateTime.UtcNow.AddMinutes(-5);
            Stored(live: true, lastStreamStart: DateTime.UtcNow.AddDays(-1), dailyPointsAllowed: true);

            await _eventHandlerService.HandleStreamLiveOnStartup(BroadcasterId, BroadcasterName, startedAt);

            VerifyWentLive(Times.Once());
            _configHelperService.Verify(x => x.UpdateDailyPointsStatus(BroadcasterId, false), Times.Once(), "yesterday's daily points should close");
        }

        [Test]
        public async Task StreamTheBotMissedStarting_LongAgo_OpensDailyPointsStraightAway()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-45);
            Stored(live: false, lastStreamStart: DateTime.UtcNow.AddDays(-1), dailyPointsAllowed: false);

            await _eventHandlerService.HandleStreamLiveOnStartup(BroadcasterId, BroadcasterName, startedAt);

            VerifyWentLive(Times.Once());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(BroadcasterId), Times.Once());
            _hoursDataService.Verify(x => x.ResetStreamMinutesForBroadcaster(It.IsAny<string>()), Times.Never());
        }
    }
}
