using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Linking;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class LinkingDataServiceTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private LinkingDataService _linkingDataService;
        private Mock<IDiscordHelperService> _discordHelperServiceMock;
        private Mock<IDiscordRoleManagerService> _discordRoleManagerService;
        private Mock<IConfigHelperService> _configHelperServiceMock;

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

            // Seed initial data
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            // Mock the DiscordHelperService
            _discordHelperServiceMock = new Mock<IDiscordHelperService>();
            _discordRoleManagerService = new Mock<IDiscordRoleManagerService>();
            _configHelperServiceMock = new Mock<IConfigHelperService>();

            _linkingDataService = new LinkingDataService(_dbContext, _discordHelperServiceMock.Object, _discordRoleManagerService.Object, _configHelperServiceMock.Object);
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
        public async Task HandleLinkCommand_DiscordNotEnabled_ReturnsNull()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link 12345", "msg1", ["!link", "12345"],
                broadcasterChannelId: DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId,
                broadcasterChannelName: DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName);

            var result = await _linkingDataService.HandleLinkCommand(msgParams);

            Assert.That(result, Is.Null);
            _discordHelperServiceMock.Verify(x => x.AddDiscordUserToDatabaseFromTwitch(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task HandleLinkCommand_NoLinkCodeProvided_ReturnsNoRequestMessage()
        {
            // mock the config helper to return discord enabled
            _configHelperServiceMock.Setup(x => x.IsDiscordEnabled(It.IsAny<string>())).Returns(true);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link", "msg2", ["!link"]);
            var result = await _linkingDataService.HandleLinkCommand(msgParams);

            Assert.That(result, Is.EqualTo("You have not requested to link your account yet. Please use /link in the discord server to link your account."));
            _discordHelperServiceMock.Verify(x => x.AddDiscordUserToDatabaseFromTwitch(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task HandleLinkCommand_InvalidLinkCodeFormat_ReturnsInvalidCodeMessage()
        {
            // mock the config helper to return discord enabled
            _configHelperServiceMock.Setup(x => x.IsDiscordEnabled(It.IsAny<string>())).Returns(true);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link abcde", "msg3", ["!link", "abcde"]);
            var result = await _linkingDataService.HandleLinkCommand(msgParams);

            Assert.That(result, Is.EqualTo("The link code is not valid. Please double check the code and try again"));
            _discordHelperServiceMock.Verify(x => x.AddDiscordUserToDatabaseFromTwitch(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task HandleLinkCommand_NoLinkRequestFound_ReturnsNoRequestMessage()
        {
            // mock the config helper to return discord enabled
            _configHelperServiceMock.Setup(x => x.IsDiscordEnabled(It.IsAny<string>())).Returns(true);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link 12345", "msg4", ["!link", "12345"],
                chatterChannelId: DatabaseSeedHelper.Channel1User2TwitchUserId,
                chatterChannelName: DatabaseSeedHelper.Channel1User2TwitchUsername);

            var result = await _linkingDataService.HandleLinkCommand(msgParams);

            Assert.That(result, Is.EqualTo("You have not requested to link your account yet. Please use /link in the discord server to link your account."));
            _discordHelperServiceMock.Verify(x => x.AddDiscordUserToDatabaseFromTwitch(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task HandleLinkCommand_IncorrectLinkCode_ReturnsInvalidCodeMessage()
        {
            // mock the config helper to return discord enabled
            _configHelperServiceMock.Setup(x => x.IsDiscordEnabled(It.IsAny<string>())).Returns(true);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link 99999", "msg5", ["!link", "99999"]); // Use incorrect code
            var result = await _linkingDataService.HandleLinkCommand(msgParams);

            Assert.That(result, Is.EqualTo("The link code is not valid. Please double check the code and try again"));
            _discordHelperServiceMock.Verify(x => x.AddDiscordUserToDatabaseFromTwitch(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task HandleLinkCommand_ValidLinkCode_LinksAccountAndReturnsSuccessMessage()
        {
            // mock the config helper to return discord enabled
            _configHelperServiceMock.Setup(x => x.IsDiscordEnabled(It.IsAny<string>())).Returns(true);

            var expectedDiscordId = 1234567890UL;
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link 12345", "msg6", ["!link", "12345"]); // Use correct code
            var result = await _linkingDataService.HandleLinkCommand(msgParams);

            Assert.That(result, Is.EqualTo("Your Twitch and Discord have been linked!"));

            var user = await _dbContext.ChannelUsers.FirstAsync(u => u.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            Assert.That(user.DiscordUserId, Is.EqualTo(expectedDiscordId));
            _discordHelperServiceMock.Verify(x => x.AddDiscordUserToDatabaseFromTwitch(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }
    }
}