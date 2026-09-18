using BreganTwitchBot.Domain.DTOs.Twitch.Api;
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
    public class ChannelInfoTests
    {
        private Mock<ITwitchApiConnection> _twitchApiConnection;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;
        private Mock<ITwitchHelperService> _twitchHelperService;

        private ChannelInfoDataService _channelInfoDataService;

        private const string CurrentTitle = "the current stream title";
        private const string CurrentGameId = "509658";
        private const string CurrentGameName = "Just Chatting";

        [SetUp]
        public void Setup()
        {
            _twitchApiConnection = new Mock<ITwitchApiConnection>();
            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();
            _twitchHelperService = new Mock<ITwitchHelperService>();

            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), "", "", "", "", AccountType.Broadcaster, 1));

            _twitchApiInteractionService.Setup(x => x.GetChannelInformationAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .ReturnsAsync(new GetChannelInformationResponse
                {
                    Title = CurrentTitle,
                    GameId = CurrentGameId,
                    GameName = CurrentGameName
                });

            _channelInfoDataService = new ChannelInfoDataService(_twitchApiConnection.Object, _twitchApiInteractionService.Object, _twitchHelperService.Object);
        }

        private static ChannelChatMessageReceivedParams CreateMsgParams(string message, bool isMod = true, bool isBroadcaster = false)
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
                IsMod = isMod,
                IsSub = false,
                IsVip = false,
                IsBroadcaster = isBroadcaster
            };
        }

        private void MakeCallerNonMod()
        {
            _twitchHelperService
                .Setup(x => x.EnsureUserHasModeratorPermissions(false, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("You don't have permission to do that"));
        }

        // Reading - available to anyone

        [Test]
        public async Task GetTitle_ReturnsCurrentTitle()
        {
            var response = await _channelInfoDataService.GetTitle(CreateMsgParams("!title", isMod: false));

            Assert.That(response, Does.Contain($"The current stream title is {CurrentTitle}"));
        }

        [Test]
        public async Task GetGame_ReturnsCurrentGame()
        {
            var response = await _channelInfoDataService.GetGame(CreateMsgParams("!game", isMod: false));

            Assert.That(response, Does.Contain($"The current game is {CurrentGameName}"));
        }

        [Test]
        public async Task GetTitle_NoBroadcasterClient_ReturnsErrorMessage()
        {
            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(It.IsAny<string>())).Returns(value: null);

            var response = await _channelInfoDataService.GetTitle(CreateMsgParams("!title"));

            Assert.That(response, Does.Contain("could not get the channel title"));
        }

        [Test]
        public async Task GetGame_ApiThrows_ReturnsErrorMessage()
        {
            _twitchApiInteractionService.Setup(x => x.GetChannelInformationAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("twitch is having a moment"));

            var response = await _channelInfoDataService.GetGame(CreateMsgParams("!game"));

            Assert.That(response, Does.Contain("could not get the game"));
        }

        // Setting - mods only

        [Test]
        public async Task SetTitle_AsMod_UpdatesTitleAndKeepsGame()
        {
            var response = await _channelInfoDataService.SetTitle(CreateMsgParams("!title a brand new title"), "a brand new title");

            // the existing game id must be sent back or twitch clears the category
            _twitchApiInteractionService.Verify(x => x.ModifyChannelInformationAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, "a brand new title", CurrentGameId), Times.Once);
            Assert.That(response, Does.Contain("The stream title has been updated to a brand new title"));
        }

        [Test]
        public void SetTitle_AsNonMod_ThrowsUnauthorised()
        {
            MakeCallerNonMod();

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _channelInfoDataService.SetTitle(CreateMsgParams("!title nope", isMod: false), "nope"));
        }

        [Test]
        public void SetTitle_AsNonMod_DoesNotCallTheApi()
        {
            MakeCallerNonMod();

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _channelInfoDataService.SetTitle(CreateMsgParams("!title nope", isMod: false), "nope"));

            _twitchApiInteractionService.Verify(x => x.ModifyChannelInformationAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SetGame_AsMod_UpdatesGameAndKeepsTitle()
        {
            _twitchApiInteractionService.Setup(x => x.GetGameByNameAsync(It.IsAny<TwitchAPI>(), "Minecraft"))
                .ReturnsAsync(("27471", "Minecraft"));

            var response = await _channelInfoDataService.SetGame(CreateMsgParams("!game Minecraft"), "Minecraft");

            // the existing title must be sent back or twitch clears it
            _twitchApiInteractionService.Verify(x => x.ModifyChannelInformationAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, CurrentTitle, "27471"), Times.Once);
            Assert.That(response, Does.Contain("The game has been updated to Minecraft"));
        }

        [Test]
        public async Task SetGame_UnknownGame_ReturnsNotFoundAndDoesNotUpdate()
        {
            // the old bot indexed [0] after only a null check, so this case threw
            _twitchApiInteractionService.Setup(x => x.GetGameByNameAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>()))
                .ReturnsAsync(((string, string)?)null);

            var response = await _channelInfoDataService.SetGame(CreateMsgParams("!game notarealgame"), "notarealgame");

            Assert.Multiple(() =>
            {
                Assert.That(response, Does.Contain("could not find a game called notarealgame"));
            });

            _twitchApiInteractionService.Verify(x => x.ModifyChannelInformationAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SetGame_AsNonMod_ThrowsUnauthorised()
        {
            MakeCallerNonMod();

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _channelInfoDataService.SetGame(CreateMsgParams("!game nope", isMod: false), "nope"));
        }

        [Test]
        public async Task SetTitle_AsBroadcaster_IsAllowed()
        {
            var response = await _channelInfoDataService.SetTitle(CreateMsgParams("!title streamer title", isMod: false, isBroadcaster: true), "streamer title");

            Assert.That(response, Does.Contain("The stream title has been updated"));
        }

        [Test]
        public async Task SetTitle_ModifyThrows_ReturnsErrorMessage()
        {
            _twitchApiInteractionService.Setup(x => x.ModifyChannelInformationAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("no permission"));

            var response = await _channelInfoDataService.SetTitle(CreateMsgParams("!title new title"), "new title");

            Assert.That(response, Does.Contain("couldn't update the title"));
        }
    }
}
