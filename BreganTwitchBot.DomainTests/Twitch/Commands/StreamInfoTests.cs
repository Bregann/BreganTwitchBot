using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Commands.StreamInfo;
using BreganTwitchBot.DomainTests.Helpers;
using Moq;
using TwitchLib.Api;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class StreamInfoTests
    {
        private Mock<ITwitchApiConnection> _twitchApiConnection;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;

        private StreamInfoDataService _streamInfoDataService;

        [SetUp]
        public void Setup()
        {
            _twitchApiConnection = new Mock<ITwitchApiConnection>();
            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();

            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), "", "", "", "", AccountType.Broadcaster, 1));

            _streamInfoDataService = new StreamInfoDataService(_twitchApiConnection.Object, _twitchApiInteractionService.Object);
        }

        private static ChannelChatMessageReceivedParams CreateMsgParams(string message)
        {
            return new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                Message = message,
                MessageParts = message.Split(' '),
                MessageId = "messageid",
                IsMod = false,
                IsSub = false,
                IsVip = false,
                IsBroadcaster = false
            };
        }

        [Test]
        public async Task GetFollowerCount_ReturnsFormattedCount()
        {
            _twitchApiInteractionService.Setup(x => x.GetChannelFollowerCount(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, It.IsAny<string>()))
                .ReturnsAsync(12345);

            var response = await _streamInfoDataService.GetFollowerCount(CreateMsgParams("!followers"));

            Assert.That(response, Does.Contain($"{DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName} has 12,345 followers"));
        }

        [Test]
        public async Task GetFollowerCount_ZeroFollowers_StillReportsTheCount()
        {
            // the pre existing GetChannelFollowersAsync returns null on an empty result, which
            // would have lost the total - the count method must report 0 rather than error
            _twitchApiInteractionService.Setup(x => x.GetChannelFollowerCount(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(0);

            var response = await _streamInfoDataService.GetFollowerCount(CreateMsgParams("!followers"));

            Assert.That(response, Does.Contain("has 0 followers"));
        }

        [Test]
        public async Task GetFollowerCount_ApiThrows_ReturnsErrorMessage()
        {
            _twitchApiInteractionService.Setup(x => x.GetChannelFollowerCount(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("twitch is having a moment"));

            var response = await _streamInfoDataService.GetFollowerCount(CreateMsgParams("!followers"));

            Assert.That(response, Does.Contain("oh no something broke"));
        }

        [Test]
        public async Task GetFollowerCount_NoApiClient_ReturnsErrorMessage()
        {
            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(It.IsAny<string>()))
                .Returns(value: null);

            var response = await _streamInfoDataService.GetFollowerCount(CreateMsgParams("!followers"));

            Assert.That(response, Does.Contain("oh no something broke"));
        }

        [Test]
        public async Task GetSubscriberCount_ReturnsFormattedCount()
        {
            _twitchApiInteractionService.Setup(x => x.GetChannelSubscriberCount(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .ReturnsAsync(4200);

            var response = await _streamInfoDataService.GetSubscriberCount(CreateMsgParams("!subs"));

            Assert.That(response, Does.Contain($"{DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName} has 4,200 subs"));
        }

        [Test]
        public async Task GetSubscriberCount_ApiThrows_ReturnsErrorMessage()
        {
            _twitchApiInteractionService.Setup(x => x.GetChannelSubscriberCount(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("no permission"));

            var response = await _streamInfoDataService.GetSubscriberCount(CreateMsgParams("!subs"));

            Assert.That(response, Does.Contain("error getting the sub count"));
        }

        [Test]
        public async Task GetSubscriberCount_NoBroadcasterClient_ReturnsErrorMessage()
        {
            // subs need the broadcaster's own token, so no broadcaster client means no sub count
            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(It.IsAny<string>()))
                .Returns(value: null);

            var response = await _streamInfoDataService.GetSubscriberCount(CreateMsgParams("!subs"));

            Assert.That(response, Does.Contain("error getting the sub count"));
        }

        [Test]
        public async Task GetSubscriberCount_UsesBroadcasterTokenNotBotToken()
        {
            _twitchApiInteractionService.Setup(x => x.GetChannelSubscriberCount(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            await _streamInfoDataService.GetSubscriberCount(CreateMsgParams("!subs"));

            _twitchApiConnection.Verify(x => x.GetBroadcasterApiClientFromChannelName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName), Times.Once);
            _twitchApiConnection.Verify(x => x.GetBotApiClient(), Times.Never);
        }
    }
}
