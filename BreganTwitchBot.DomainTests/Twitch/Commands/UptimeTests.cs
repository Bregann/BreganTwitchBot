using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Commands.Uptime;
using BreganTwitchBot.DomainTests.Helpers;
using Moq;
using TwitchLib.Api;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class UptimeTests
    {
        private Mock<ITwitchApiConnection> _twitchApiConnection;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;

        private UptimeDataService _uptimeDataService;

        [SetUp]
        public void Setup()
        {
            _twitchApiConnection = new Mock<ITwitchApiConnection>();
            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();

            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), "", "", "", "", AccountType.Broadcaster, 1));

            _uptimeDataService = new UptimeDataService(_twitchApiConnection.Object, _twitchApiInteractionService.Object);
        }

        private static ChannelChatMessageReceivedParams CreateMsgParams()
        {
            return new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                Message = "!uptime",
                MessageParts = ["!uptime"],
                MessageId = "messageid",
                IsMod = false,
                IsSub = false,
                IsVip = false,
                IsBroadcaster = false
            };
        }

        [Test]
        public async Task GetStreamUptime_StreamLive_ReturnsUptime()
        {
            _twitchApiInteractionService.Setup(x => x.GetStreams(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .ReturnsAsync(new GetStreamsResponse
                {
                    Id = "streamid",
                    GameId = "1",
                    GameName = "Just Chatting",
                    ViewerCount = 10,
                    StartedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(-30)
                });

            var response = await _uptimeDataService.GetStreamUptime(CreateMsgParams());

            Assert.Multiple(() =>
            {
                Assert.That(response, Does.Contain("has been streaming for"));
                Assert.That(response, Does.Contain("2 hours"));
                Assert.That(response, Does.Contain("30 minutes"));
            });
        }

        [Test]
        public async Task GetStreamUptime_StreamOffline_ReturnsNotLive()
        {
            _twitchApiInteractionService.Setup(x => x.GetStreams(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync((GetStreamsResponse?)null);

            var response = await _uptimeDataService.GetStreamUptime(CreateMsgParams());

            Assert.That(response, Is.EqualTo($"{DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName} is not live :("));
        }

        [Test]
        public async Task GetStreamUptime_ApiThrows_ReturnsErrorMessage()
        {
            _twitchApiInteractionService.Setup(x => x.GetStreams(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("twitch is having a moment"));

            var response = await _uptimeDataService.GetStreamUptime(CreateMsgParams());

            Assert.That(response, Does.Contain("Error getting uptime"));
        }

        [Test]
        public async Task GetStreamUptime_NoApiClient_ReturnsErrorMessage()
        {
            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(It.IsAny<string>()))
                .Returns(value: null);

            var response = await _uptimeDataService.GetStreamUptime(CreateMsgParams());

            Assert.That(response, Does.Contain("Error getting uptime"));
        }

        [Test]
        public void GetBotUptime_ReturnsUptimeMessage()
        {
            var response = _uptimeDataService.GetBotUptime(CreateMsgParams());

            Assert.That(response, Does.Contain("The bot has been up for"));
        }

        // DurationFormatHelper - shared by uptime and anything else needing chat friendly durations

        [Test]
        public void Humanise_ZeroDuration_ReturnsSeconds()
        {
            Assert.That(Domain.Services.Helpers.DurationFormatHelper.Humanise(TimeSpan.Zero), Is.EqualTo("0 seconds"));
        }

        [Test]
        public void Humanise_NegativeDuration_ClampsToZero()
        {
            Assert.That(Domain.Services.Helpers.DurationFormatHelper.Humanise(TimeSpan.FromSeconds(-30)), Is.EqualTo("0 seconds"));
        }

        [Test]
        public void Humanise_SingularUnits_DoesNotPluralise()
        {
            var timeSpan = new TimeSpan(1, 1, 1, 1);
            Assert.That(Domain.Services.Helpers.DurationFormatHelper.Humanise(timeSpan), Is.EqualTo("1 day, 1 hour, 1 minute and 1 second"));
        }

        [Test]
        public void Humanise_SkipsZeroUnits()
        {
            // 2 hours 0 minutes 5 seconds - the minutes should not appear
            var timeSpan = new TimeSpan(2, 0, 5);
            Assert.That(Domain.Services.Helpers.DurationFormatHelper.Humanise(timeSpan), Is.EqualTo("2 hours and 5 seconds"));
        }

        [Test]
        public void Humanise_OverAYear_ReportsYearsAndDays()
        {
            var timeSpan = TimeSpan.FromDays(400);
            var result = Domain.Services.Helpers.DurationFormatHelper.Humanise(timeSpan);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("1 year"));
                Assert.That(result, Does.Contain("35 days"));
            });
        }

        [Test]
        public void Humanise_MinutesOnly_ReadsNaturally()
        {
            Assert.That(Domain.Services.Helpers.DurationFormatHelper.Humanise(TimeSpan.FromMinutes(5)), Is.EqualTo("5 minutes"));
        }
    }
}
