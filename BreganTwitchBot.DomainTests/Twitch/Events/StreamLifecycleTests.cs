using BreganTwitchBot.Domain.DTOs.Helpers;
using BreganTwitchBot.Domain.DTOs.Twitch.Api;
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
    /// Every way a stream and the bot can start, stop and restart. Only what's stored survives the
    /// bot restarting, so each test sets up what was stored and plays out what happens next.
    /// </summary>
    [TestFixture]
    public class StreamLifecycleTests
    {
        private const string BroadcasterId = "123";
        private const string BroadcasterName = "coolstreamername";

        private StreamState _stored;

        private Mock<IConfigHelperService> _configHelperService;
        private Mock<ITwitchHelperService> _twitchHelperService;
        private Mock<IDiscordHelperService> _discordHelperService;
        private Mock<IStreamStatsService> _streamStatsService;
        private Mock<IDailyPointsDataService> _dailyPointsDataService;
        private Mock<IHoursDataService> _hoursDataService;
        private ServiceProvider _serviceProvider;

        private TwitchEventHandlerService _eventHandlerService;

        [SetUp]
        public void Setup()
        {
            JobStorage.Current = new MemoryStorage();

            // never been live
            _stored = new StreamState(false, null, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-7), false, DateTime.UtcNow.AddDays(-7));

            // the stored state changes the way the real config does, so a test can play out a sequence
            _configHelperService = new Mock<IConfigHelperService>();
            _configHelperService.Setup(x => x.GetStreamState(BroadcasterId)).Returns(() => _stored);
            _configHelperService.Setup(x => x.MarkStreamLive(BroadcasterId, It.IsAny<string>(), It.IsAny<DateTime?>()))
                .Callback<string, string, DateTime?>((_, streamId, started) => _stored = _stored with { Live = true, TwitchStreamId = streamId, LastStreamStart = started ?? _stored.LastStreamStart })
                .Returns(Task.CompletedTask);
            _configHelperService.Setup(x => x.UpdateStreamLiveStatus(BroadcasterId, false))
                .Callback(() => _stored = _stored with { Live = false, LastStreamEnd = DateTime.UtcNow })
                .Returns(Task.CompletedTask);
            _configHelperService.Setup(x => x.UpdateDailyPointsStatus(BroadcasterId, It.IsAny<bool>()))
                .Callback<string, bool>((_, allowed) => _stored = _stored with { DailyPointsAllowed = allowed })
                .Returns(Task.CompletedTask);
            _configHelperService.Setup(x => x.IsDiscordEnabled(BroadcasterId)).Returns(true);
            _configHelperService.Setup(x => x.GetDiscordConfig(BroadcasterId)).Returns(new DiscordConfig { DiscordStreamAnnouncementChannelId = 999 });

            // opening the points marks them opened today, like the real one
            _dailyPointsDataService = new Mock<IDailyPointsDataService>();
            _dailyPointsDataService.Setup(x => x.AllowDailyPointsCollecting(BroadcasterId))
                .Callback(() => _stored = _stored with { DailyPointsAllowed = true, LastDailyPointsAllowed = DateTime.UtcNow })
                .Returns(Task.CompletedTask);

            _twitchHelperService = new Mock<ITwitchHelperService>();
            _discordHelperService = new Mock<IDiscordHelperService>();
            _streamStatsService = new Mock<IStreamStatsService>();
            _hoursDataService = new Mock<IHoursDataService>();

            var services = new ServiceCollection();
            services.AddSingleton(_dailyPointsDataService.Object);
            services.AddSingleton(_hoursDataService.Object);
            _serviceProvider = services.BuildServiceProvider();

            _eventHandlerService = CreateHandler();
        }

        /// <summary>
        /// A fresh handler, as the bot has after restarting
        /// </summary>
        private TwitchEventHandlerService CreateHandler()
        {
            return new TwitchEventHandlerService(
                _twitchHelperService.Object,
                new Mock<ITwitchApiInteractionService>().Object,
                new Mock<ITwitchApiConnection>().Object,
                _configHelperService.Object,
                _discordHelperService.Object,
                _serviceProvider,
                _streamStatsService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _serviceProvider.Dispose();
        }

        private void RestartBot()
        {
            _eventHandlerService = CreateHandler();
            JobStorage.Current = new MemoryStorage();
            _discordHelperService.Invocations.Clear();
            _streamStatsService.Invocations.Clear();
            _hoursDataService.Invocations.Clear();
            _dailyPointsDataService.Invocations.Clear();
        }

        private static GetStreamsResponse Twitch(string streamId, DateTime startedAt) => new()
        {
            Id = streamId,
            StartedAt = startedAt,
            GameId = "1",
            GameName = "Just Chatting",
            ViewerCount = 10
        };

        private void VerifyNewBroadcast(Times times)
        {
            _discordHelperService.Verify(x => x.SendMessage(It.IsAny<ulong>(), It.Is<string>(msg => msg.Contains("@everyone"))), times);
            _hoursDataService.Verify(x => x.ResetStreamMinutesForBroadcaster(BroadcasterId), times);
            _streamStatsService.Verify(x => x.StartNewStream(BroadcasterId), times);
        }

        private static int ScheduledJobs() => (int)JobStorage.Current.GetMonitoringApi().ScheduledCount();

        // going live

        [Test]
        public async Task NewStream_GoesLive_AndSchedulesDailyPoints()
        {
            var startedAt = DateTime.UtcNow;

            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);

            VerifyNewBroadcast(Times.Once());
            Assert.Multiple(() =>
            {
                Assert.That(_stored.Live, Is.True);
                Assert.That(_stored.TwitchStreamId, Is.EqualTo("stream1"));
                Assert.That(_stored.LastStreamStart, Is.EqualTo(startedAt));
                Assert.That(_stored.DailyPointsAllowed, Is.False, "they open 30 minutes in");
                Assert.That(ScheduledJobs(), Is.EqualTo(2), "daily points and the boss countdown");
            });
        }

        [Test]
        public async Task NewStream_NoticedLate_OpensDailyPointsStraightAway()
        {
            // e.g. the bot was down when it started
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", DateTime.UtcNow.AddMinutes(-45));

            VerifyNewBroadcast(Times.Once());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task SecondStreamTheSameDay_OpensDailyPointsStraightAway()
        {
            // they've already reset streaks today, so there's nothing to wait for
            _stored = _stored with { LastDailyPointsAllowed = DateTime.UtcNow.Date, LastStreamEnd = DateTime.UtcNow.AddHours(-3) };

            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream2", DateTime.UtcNow);

            VerifyNewBroadcast(Times.Once());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task SameStreamTwice_OnlyGoesLiveOnce()
        {
            var startedAt = DateTime.UtcNow;

            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);

            VerifyNewBroadcast(Times.Once());
        }

        [Test]
        public async Task TheEventAndTheCheckArrivingTogether_OnlyGoLiveOnce()
        {
            var startedAt = DateTime.UtcNow;

            await Task.WhenAll(
                _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt),
                _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", startedAt)));

            VerifyNewBroadcast(Times.Once());
        }

        // the bot restarting

        [Test]
        public async Task BotRestart_AfterDailyPointsOpenedAndClaimed_ChangesNothing()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-50);
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);
            Assert.That(_stored.DailyPointsAllowed, Is.True, "opened as it's past 30 minutes");

            RestartBot();
            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", startedAt));

            VerifyNewBroadcast(Times.Never());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(It.IsAny<string>()), Times.Never(), "open points aren't touched, so claims and streaks stay as they are");
            _configHelperService.Verify(x => x.UpdateDailyPointsStatus(BroadcasterId, false), Times.Once(), "only closed once, when the stream first went live");
            Assert.That(_stored.DailyPointsAllowed, Is.True);
        }

        [Test]
        public async Task BotRestart_BeforeDailyPointsOpen_ReschedulesThemInsteadOfOpeningEarly()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-5);
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);

            RestartBot();
            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", startedAt));

            VerifyNewBroadcast(Times.Never());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(It.IsAny<string>()), Times.Never());
            Assert.That(ScheduledJobs(), Is.EqualTo(1), "just the daily points, not another boss countdown");
        }

        [Test]
        public async Task BotRestart_AfterDailyPointsShouldHaveOpened_OpensThemNow()
        {
            // the bot was down when they were due
            _stored = new StreamState(true, "stream1", DateTime.UtcNow.AddMinutes(-45), DateTime.UtcNow.AddDays(-1), false, DateTime.UtcNow.AddDays(-1));

            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", DateTime.UtcNow.AddMinutes(-45)));

            VerifyNewBroadcast(Times.Never());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task FirstRestartAfterStreamIdsWereAdded_MidStream_CarriesOn()
        {
            // deploying this change mid stream: live, but no stream id stored yet
            var startedAt = DateTime.UtcNow.AddMinutes(-50);
            _stored = new StreamState(true, null, startedAt, DateTime.UtcNow.AddDays(-1), true, DateTime.UtcNow.AddMinutes(-20));

            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", startedAt));

            VerifyNewBroadcast(Times.Never());
            _streamStatsService.Verify(x => x.EndStream(It.IsAny<string>()), Times.Never());
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(It.IsAny<string>()), Times.Never());
            Assert.That(_stored.TwitchStreamId, Is.EqualTo("stream1"));
        }

        [Test]
        public async Task BotDownWhileTheStreamEnded_EndsItAfterEnoughChecks()
        {
            _stored = new StreamState(true, "stream1", DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddDays(-1), true, DateTime.UtcNow.AddHours(-2));

            for (var i = 1; i < TwitchEventHandlerService.OfflineChecksNeeded; i++)
            {
                await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, null);
                Assert.That(_stored.Live, Is.True, $"one failed check shouldn't end a stream (check {i})");
            }

            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, null);

            Assert.Multiple(() =>
            {
                Assert.That(_stored.Live, Is.False, "so watchtime, !spin and !daily stop");
                Assert.That(_stored.DailyPointsAllowed, Is.False);
            });
            _streamStatsService.Verify(x => x.EndStream(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task ACheckThatFindsTheStreamAgain_StartsTheOfflineCountAgain()
        {
            _stored = new StreamState(true, "stream1", DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddDays(-1), true, DateTime.UtcNow.AddHours(-2));

            for (var i = 1; i < TwitchEventHandlerService.OfflineChecksNeeded; i++)
            {
                await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, null);
            }

            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", DateTime.UtcNow.AddHours(-3)));
            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, null);

            Assert.That(_stored.Live, Is.True);
        }

        [Test]
        public async Task JustAfterGoingLive_TwitchNotShowingItYet_DoesNotEndIt()
        {
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", DateTime.UtcNow);

            for (var i = 0; i < TwitchEventHandlerService.OfflineChecksNeeded + 2; i++)
            {
                await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, null);
            }

            Assert.That(_stored.Live, Is.True);
        }

        [Test]
        public async Task BotDownWhileTheStreamEndedAndANewOneStarted_StartsANewBroadcast()
        {
            // still marked live on yesterday's stream
            _stored = new StreamState(true, "stream1", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2), true, DateTime.UtcNow.AddDays(-1));

            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream2", DateTime.UtcNow.AddMinutes(-5)));

            _streamStatsService.Verify(x => x.EndStream(BroadcasterId), Times.Once(), "yesterday's stats are closed off");
            VerifyNewBroadcast(Times.Once());
            Assert.Multiple(() =>
            {
                Assert.That(_stored.TwitchStreamId, Is.EqualTo("stream2"));
                Assert.That(_stored.DailyPointsAllowed, Is.False, "yesterday's points are closed until today's open");
            });
        }

        // the stream restarting

        [Test]
        public async Task StreamOffline_ClosesEverything()
        {
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", DateTime.UtcNow.AddMinutes(-60));

            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);

            Assert.Multiple(() =>
            {
                Assert.That(_stored.Live, Is.False);
                Assert.That(_stored.DailyPointsAllowed, Is.False);
            });
            _streamStatsService.Verify(x => x.EndStream(BroadcasterId), Times.Once());
            _twitchHelperService.Verify(x => x.ClearStreamChattersList(BroadcasterId), Times.Exactly(2), "on going live and going offline");
        }

        [Test]
        public async Task StreamOfflineTwice_OnlyEndsOnce()
        {
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", DateTime.UtcNow.AddMinutes(-60));

            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);
            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);

            _streamStatsService.Verify(x => x.EndStream(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task StreamDropsAndComesBack_AfterDailyPointsOpened_CarriesOnWithoutResetting()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-60);
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);
            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);
            _discordHelperService.Invocations.Clear();
            _hoursDataService.Invocations.Clear();
            _streamStatsService.Invocations.Clear();
            _dailyPointsDataService.Invocations.Clear();

            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream2", DateTime.UtcNow);

            VerifyNewBroadcast(Times.Never());
            Assert.Multiple(() =>
            {
                Assert.That(_stored.Live, Is.True);
                Assert.That(_stored.TwitchStreamId, Is.EqualTo("stream2"));
                Assert.That(_stored.LastStreamStart, Is.EqualTo(startedAt), "it's still the same broadcast");
            });

            // reopened straight away. Having opened today, they reopen without resetting streaks
            // and claims are kept, which DailyPointsTests covers
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(BroadcasterId), Times.Once());
        }

        [Test]
        public async Task StreamDropsAndComesBack_BeforeDailyPointsOpened_KeepsTheOriginalTime()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-10);
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);
            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);
            JobStorage.Current = new MemoryStorage();

            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream2", DateTime.UtcNow);

            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(It.IsAny<string>()), Times.Never());

            var scheduled = JobStorage.Current.GetMonitoringApi().ScheduledJobs(0, 10).Single().Value;
            Assert.That(scheduled.EnqueueAt, Is.EqualTo(startedAt + TwitchEventHandlerService.DailyPointsDelay).Within(TimeSpan.FromSeconds(5)), "30 minutes after the stream first started, not after it came back");
        }

        [Test]
        public async Task StreamComesBackAfterTheReconnectWindow_IsANewBroadcast()
        {
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", DateTime.UtcNow.AddHours(-3));
            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);
            _discordHelperService.Invocations.Clear();
            _hoursDataService.Invocations.Clear();
            _streamStatsService.Invocations.Clear();

            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream2", DateTime.UtcNow + TwitchEventHandlerService.ReconnectWindow + TimeSpan.FromMinutes(1));

            VerifyNewBroadcast(Times.Once());
        }

        [Test]
        public async Task StreamDropsAndComesBackWhileTheBotRestarts_CarriesOn()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-60);
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);
            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);

            RestartBot();
            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream2", DateTime.UtcNow));

            VerifyNewBroadcast(Times.Never());
            Assert.That(_stored.LastStreamStart, Is.EqualTo(startedAt));
        }

        [Test]
        public async Task TwitchStillShowingAStreamThatJustEnded_DoesNotBringItBack()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-60);
            await _eventHandlerService.HandleStreamOnline(BroadcasterId, BroadcasterName, "stream1", startedAt);
            await _eventHandlerService.HandleStreamOffline(BroadcasterId, BroadcasterName);

            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", startedAt));

            Assert.That(_stored.Live, Is.False);
        }

        [Test]
        public async Task AStreamEndedByMistake_IsCarriedOnWhenItsStillGoing()
        {
            // ended after twitch's api failed several times, and long enough ago that it isn't api lag
            var startedAt = DateTime.UtcNow.AddHours(-2);
            _stored = new StreamState(false, "stream1", startedAt, DateTime.UtcNow.AddMinutes(-15), false, DateTime.UtcNow.AddHours(-1));

            await _eventHandlerService.CheckStreamStatus(BroadcasterId, BroadcasterName, Twitch("stream1", startedAt));

            VerifyNewBroadcast(Times.Never());
            Assert.That(_stored.Live, Is.True);
            _dailyPointsDataService.Verify(x => x.AllowDailyPointsCollecting(BroadcasterId), Times.Once());
        }
    }
}
