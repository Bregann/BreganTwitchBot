using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Commands.Clip;
using BreganTwitchBot.DomainTests.Helpers;
using Moq;
using TwitchLib.Api;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class ClipTests
    {
        private Mock<ITwitchApiConnection> _twitchApiConnection;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;

        private ClipDataService _clipDataService;

        [SetUp]
        public void Setup()
        {
            _twitchApiConnection = new Mock<ITwitchApiConnection>();
            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();

            _twitchApiConnection.Setup(x => x.GetBotApiClient())
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), "", "", "", "", AccountType.Bot));

            _twitchApiInteractionService.Setup(x => x.GetStreams(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .ReturnsAsync(new GetStreamsResponse
                {
                    Id = "streamid",
                    GameId = "1",
                    GameName = "Just Chatting",
                    ViewerCount = 10,
                    StartedAt = DateTime.UtcNow.AddHours(-1)
                });

            _clipDataService = new ClipDataService(_twitchApiConnection.Object, _twitchApiInteractionService.Object);
        }

        private static ChannelChatMessageReceivedParams CreateMsgParams()
        {
            return new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                Message = "!clip",
                MessageParts = ["!clip"],
                MessageId = "messageid",
                IsMod = false,
                IsSub = false,
                IsVip = false,
                IsBroadcaster = false
            };
        }

        [Test]
        public async Task CreateClip_StreamLive_ReturnsClipLink()
        {
            _twitchApiInteractionService.Setup(x => x.CreateClip(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .ReturnsAsync("AwkwardHelplessSalamanderSwiftRage");

            var response = await _clipDataService.CreateClip(CreateMsgParams());

            Assert.That(response, Does.Contain("https://clips.twitch.tv/AwkwardHelplessSalamanderSwiftRage"));
        }

        [Test]
        public async Task CreateClip_StreamOffline_DoesNotClip()
        {
            _twitchApiInteractionService.Setup(x => x.GetStreams(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync((GetStreamsResponse?)null);

            var response = await _clipDataService.CreateClip(CreateMsgParams());

            Assert.That(response, Does.Contain("is not live"));
            _twitchApiInteractionService.Verify(x => x.CreateClip(It.IsAny<TwitchAPI>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task CreateClip_TwitchReturnsNoClip_ReturnsErrorMessage()
        {
            _twitchApiInteractionService.Setup(x => x.CreateClip(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var response = await _clipDataService.CreateClip(CreateMsgParams());

            Assert.That(response, Does.Contain("Error creating clip"));
        }

        [Test]
        public async Task CreateClip_ApiThrows_ReturnsErrorMessage()
        {
            _twitchApiInteractionService.Setup(x => x.CreateClip(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("clips are disabled on this channel"));

            var response = await _clipDataService.CreateClip(CreateMsgParams());

            Assert.That(response, Does.Contain("Error creating clip"));
        }

        [Test]
        public async Task CreateClip_NoBotApiClient_ReturnsErrorMessage()
        {
            _twitchApiConnection.Setup(x => x.GetBotApiClient()).Returns(value: null);

            var response = await _clipDataService.CreateClip(CreateMsgParams());

            Assert.That(response, Does.Contain("Error creating clip"));
        }
    }
}
