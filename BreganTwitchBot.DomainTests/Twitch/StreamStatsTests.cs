using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;
using TwitchLib.Api;

namespace BreganTwitchBot.DomainTests.Twitch
{
    [TestFixture]
    public class StreamStatsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private ServiceProvider _serviceProvider;
        private AppDbContext _dbContext;
        private Mock<ITwitchApiConnection> _twitchApiConnection;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;
        private Mock<IDiscordHelperService> _discordHelperService;
        private Mock<IConfigHelperService> _configHelperService;

        private StreamStatsService _streamStatsService;

        private const string ChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;

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
                options.UseLazyLoadingProxies()
                       .UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped);

            _serviceProvider = services.BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _twitchApiConnection = new Mock<ITwitchApiConnection>();
            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();

            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(It.IsAny<string>()))
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), "", "", "", "", AccountType.Broadcaster, 1));

            _twitchApiInteractionService.Setup(x => x.GetChannelFollowerCount(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(1000);
            _twitchApiInteractionService.Setup(x => x.GetChannelSubscriberCount(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync(50);

            _discordHelperService = new Mock<IDiscordHelperService>();
            _configHelperService = new Mock<IConfigHelperService>();

            _streamStatsService = new StreamStatsService(_serviceProvider, _twitchApiConnection.Object, _twitchApiInteractionService.Object, _discordHelperService.Object, _configHelperService.Object);
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

        private async Task<Domain.Database.Models.TwitchStreamStats> GetCurrentStream()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.TwitchStreamStats
                .Where(x => x.Channel.BroadcasterTwitchChannelId == ChannelId)
                .OrderByDescending(x => x.StreamId)
                .FirstAsync();
        }

        [Test]
        public async Task StartNewStream_CreatesAStreamWithOpeningCounts()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            var stream = await GetCurrentStream();

            Assert.Multiple(() =>
            {
                Assert.That(stream.StreamId, Is.EqualTo(1));
                Assert.That(stream.StartingFollowerCount, Is.EqualTo(1000));
                Assert.That(stream.StartingSubscriberCount, Is.EqualTo(50));
            });
        }

        [Test]
        public async Task StartNewStream_IncrementsTheStreamId()
        {
            await _streamStatsService.StartNewStream(ChannelId);
            await _streamStatsService.StartNewStream(ChannelId);

            var stream = await GetCurrentStream();

            Assert.That(stream.StreamId, Is.EqualTo(2));
        }

        [Test]
        public async Task UpdateStreamStat_AccumulatesAndFlushes()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.MessagesReceived);
            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.MessagesReceived);
            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.BitsDonated, 500);

            await _streamStatsService.FlushStats();

            var stream = await GetCurrentStream();

            Assert.Multiple(() =>
            {
                Assert.That(stream.MessagesReceived, Is.EqualTo(2));
                Assert.That(stream.BitsDonated, Is.EqualTo(500));
            });
        }

        [Test]
        public async Task FlushStats_IsCumulativeAcrossFlushes()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.MessagesReceived, 5);
            await _streamStatsService.FlushStats();

            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.MessagesReceived, 3);
            await _streamStatsService.FlushStats();

            var stream = await GetCurrentStream();

            Assert.That(stream.MessagesReceived, Is.EqualTo(8));
        }

        [Test]
        public async Task FlushStats_ClearsPendingSoCountsAreNotDoubleApplied()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.MessagesReceived, 5);
            await _streamStatsService.FlushStats();
            await _streamStatsService.FlushStats();

            var stream = await GetCurrentStream();

            Assert.That(stream.MessagesReceived, Is.EqualTo(5));
        }

        [Test]
        public async Task StatsAreKeptPerChannel()
        {
            await _streamStatsService.StartNewStream(ChannelId);
            await _streamStatsService.StartNewStream(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);

            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.MessagesReceived, 10);
            _streamStatsService.UpdateStreamStat(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId, StreamStatType.MessagesReceived, 3);

            await _streamStatsService.FlushStats();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var channel1Stream = await context.TwitchStreamStats.Where(x => x.Channel.BroadcasterTwitchChannelId == ChannelId).OrderByDescending(x => x.StreamId).FirstAsync();
            var channel2Stream = await context.TwitchStreamStats.Where(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId).OrderByDescending(x => x.StreamId).FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(channel1Stream.MessagesReceived, Is.EqualTo(10));
                Assert.That(channel2Stream.MessagesReceived, Is.EqualTo(3));
            });
        }

        [Test]
        public async Task UniqueViewers_AreDeduplicated()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            _streamStatsService.AddUniqueViewer(ChannelId, "someviewer");
            _streamStatsService.AddUniqueViewer(ChannelId, "SomeViewer");
            _streamStatsService.AddUniqueViewer(ChannelId, "anotherviewer");

            await _streamStatsService.FlushStats();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == ChannelId);

            var count = await context.UniqueViewers.CountAsync(x => x.ChannelId == channel.Id);

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task UniqueViewers_AreNotDuplicatedAcrossFlushes()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            _streamStatsService.AddUniqueViewer(ChannelId, "someviewer");
            await _streamStatsService.FlushStats();
            await _streamStatsService.FlushStats();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == ChannelId);

            Assert.That(await context.UniqueViewers.CountAsync(x => x.ChannelId == channel.Id), Is.EqualTo(1));
        }

        [Test]
        public async Task RecordViewerCount_FeedsAverageAndPeak()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            await _streamStatsService.RecordViewerCount(ChannelId, 10);
            await _streamStatsService.RecordViewerCount(ChannelId, 30);
            await _streamStatsService.RecordViewerCount(ChannelId, 20);

            await _streamStatsService.EndStream(ChannelId);

            var stream = await GetCurrentStream();

            Assert.Multiple(() =>
            {
                Assert.That(stream.AvgViewCount, Is.EqualTo(20));
                Assert.That(stream.PeakViewerCount, Is.EqualTo(30));
            });
        }

        [Test]
        public async Task EndStream_CapturesClosingCountsAndUptime()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            _twitchApiInteractionService.Setup(x => x.GetChannelFollowerCount(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(1050);

            await _streamStatsService.EndStream(ChannelId);

            var stream = await GetCurrentStream();

            Assert.Multiple(() =>
            {
                Assert.That(stream.EndingFollowerCount, Is.EqualTo(1050));
                Assert.That(stream.EndingSubscriberCount, Is.EqualTo(50));
                Assert.That(stream.Uptime, Is.GreaterThan(TimeSpan.Zero));
                Assert.That(stream.StreamEnded, Is.GreaterThan(stream.StreamStarted));
            });
        }

        [Test]
        public async Task EndStream_FlushesPendingStatsFirst()
        {
            await _streamStatsService.StartNewStream(ChannelId);

            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.PointsWon, 999);
            await _streamStatsService.EndStream(ChannelId);

            var stream = await GetCurrentStream();

            Assert.That(stream.PointsWon, Is.EqualTo(999));
        }

        [Test]
        public async Task StartNewStream_ClearsThePreviousStreamsViewerData()
        {
            await _streamStatsService.StartNewStream(ChannelId);
            await _streamStatsService.RecordViewerCount(ChannelId, 42);
            _streamStatsService.AddUniqueViewer(ChannelId, "someviewer");
            await _streamStatsService.FlushStats();

            await _streamStatsService.StartNewStream(ChannelId);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == ChannelId);

            Assert.Multiple(async () =>
            {
                Assert.That(await context.StreamViewCounts.CountAsync(x => x.ChannelId == channel.Id), Is.EqualTo(0));
                Assert.That(await context.UniqueViewers.CountAsync(x => x.ChannelId == channel.Id), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task UpdateStreamStat_WithNoStream_DoesNotThrow()
        {
            _streamStatsService.UpdateStreamStat(ChannelId, StreamStatType.MessagesReceived);

            Assert.DoesNotThrowAsync(async () => await _streamStatsService.FlushStats());
            Assert.DoesNotThrowAsync(async () => await _streamStatsService.FlushStats());
        }

        [Test]
        public async Task SampleViewerCounts_OnlyRecordsForLiveChannels()
        {
            _twitchApiConnection.Setup(x => x.GetAllChannels())
                .Returns([new TwitchApiConnection.ChannelDetails(1, ChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)]);

            // not live
            _twitchApiInteractionService.Setup(x => x.GetStreams(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync((Domain.DTOs.Twitch.Api.GetStreamsResponse?)null);

            await _streamStatsService.StartNewStream(ChannelId);
            await _streamStatsService.SampleViewerCounts();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == ChannelId);

            Assert.That(await context.StreamViewCounts.CountAsync(x => x.ChannelId == channel.Id), Is.EqualTo(0));
        }

        [Test]
        public async Task SampleViewerCounts_RecordsTheViewerCountWhenLive()
        {
            _twitchApiConnection.Setup(x => x.GetAllChannels())
                .Returns([new TwitchApiConnection.ChannelDetails(1, ChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)]);

            _twitchApiInteractionService.Setup(x => x.GetStreams(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync(new Domain.DTOs.Twitch.Api.GetStreamsResponse
                {
                    GameId = "1",
                    GameName = "Just Chatting",
                    ViewerCount = 42,
                    StartedAt = DateTime.UtcNow
                });

            await _streamStatsService.StartNewStream(ChannelId);
            await _streamStatsService.SampleViewerCounts();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == ChannelId);

            var sample = await context.StreamViewCounts.FirstAsync(x => x.ChannelId == channel.Id);
            Assert.That(sample.ViewCount, Is.EqualTo(42));
        }

        [Test]
        public async Task ReportFollowerChanges_FirstRunOnlyRecordsTheBaseline()
        {
            _twitchApiConnection.Setup(x => x.GetAllChannels())
                .Returns([new TwitchApiConnection.ChannelDetails(1, ChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)]);
            _configHelperService.Setup(x => x.IsDiscordEnabled(It.IsAny<string>())).Returns(true);

            await _streamStatsService.ReportFollowerChanges();

            // nothing to compare against yet, so no message
            _discordHelperService.Verify(x => x.SendEmbedMessage(It.IsAny<ulong>(), It.IsAny<global::Discord.EmbedBuilder>()), Times.Never);
        }

        [Test]
        public async Task ReportFollowerChanges_NoChange_SendsNothing()
        {
            _twitchApiConnection.Setup(x => x.GetAllChannels())
                .Returns([new TwitchApiConnection.ChannelDetails(1, ChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)]);
            _configHelperService.Setup(x => x.IsDiscordEnabled(It.IsAny<string>())).Returns(true);

            await _streamStatsService.ReportFollowerChanges();
            await _streamStatsService.ReportFollowerChanges();

            _discordHelperService.Verify(x => x.SendEmbedMessage(It.IsAny<ulong>(), It.IsAny<global::Discord.EmbedBuilder>()), Times.Never);
        }
    }
}
