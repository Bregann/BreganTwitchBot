using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class WordBlacklistDataTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<ITwitchHelperService> _twitchHelperServiceMock;
        private Mock<IWordBlacklistMonitorService> _wordBlacklistMonitorServiceMock;

        private WordBlacklistDataService _wordBlacklistService;

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
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseLazyLoadingProxies()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            _dbContext = new AppDbContext(options);
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _twitchHelperServiceMock = new Mock<ITwitchHelperService>();
            _wordBlacklistMonitorServiceMock = new Mock<IWordBlacklistMonitorService>();

            _twitchHelperServiceMock
                .Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            _wordBlacklistMonitorServiceMock
                .Setup(x => x.AddWordToBlacklist(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WordType>()));

            _wordBlacklistService = new WordBlacklistDataService(_dbContext, _twitchHelperServiceMock.Object, _wordBlacklistMonitorServiceMock.Object);
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
        public void HandleAddWordCommand_UserIsNotBroadcasterOrSuperMod_ThrowsInvalidCommandException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams(
                "!addword badword",
                "msg1",
                new string[] { "!addword", "badword" },
                isBroadcaster: false
            );
            var wordType = WordType.PermBanWord;

            var ex = Assert.ThrowsAsync<InvalidCommandException>(async () => await _wordBlacklistService.HandleAddWordCommand(msgParams, wordType));

            Assert.That(ex.Message, Is.EqualTo("You do not have permission to use this command."));
            _wordBlacklistMonitorServiceMock.Verify(x => x.AddWordToBlacklist(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WordType>()), Times.Never);
        }

        [Test]
        public void HandleAddWordCommand_MessageHasLessThanTwoParts_ThrowsInvalidCommandException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams(
                "!addword",
                "msg2",
                new string[] { "!addword" },
                isBroadcaster: true
            );
            var wordType = WordType.PermBanWord;

            var ex = Assert.ThrowsAsync<InvalidCommandException>(async () => await _wordBlacklistService.HandleAddWordCommand(msgParams, wordType));
            Assert.That(ex.Message, Is.EqualTo("You must specify a word to add. Duhhh"));
            _wordBlacklistMonitorServiceMock.Verify(x => x.AddWordToBlacklist(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WordType>()), Times.Never);
        }

        [Test]
        public async Task HandleAddWordCommand_WordAlreadyExistsInBlacklist_ThrowsInvalidCommandException()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelName == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);
            var wordToAdd = "existingword";
            var wordType = WordType.PermBanWord;

            _dbContext.Blacklist.Add(new Blacklist
            {
                Word = wordToAdd,
                WordType = wordType,
                ChannelId = channel.Id
            });
            await _dbContext.SaveChangesAsync();

            var msgParams = MessageParamsHelper.CreateChatMessageParams(
                $"!addword {wordToAdd}",
                "msg3",
                new string[] { "!addword", wordToAdd },
                isBroadcaster: true
            );

            var ex = Assert.ThrowsAsync<InvalidCommandException>(async () => await _wordBlacklistService.HandleAddWordCommand(msgParams, wordType));
            Assert.That(ex.Message, Is.EqualTo($"The word is already in the blacklist!"));
            _wordBlacklistMonitorServiceMock.Verify(x => x.AddWordToBlacklist(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WordType>()), Times.Never);
        }

        [Test]
        public async Task HandleAddWordCommand_UserIsBroadcasterAndCommandIsValid_AddsWordAndReturnsSuccessMessage()
        {
            var wordToAdd = "newbadword";
            var wordType = WordType.PermBanWord;
            var msgParams = MessageParamsHelper.CreateChatMessageParams(
                $"!addword {wordToAdd}",
                "msg4",
                new string[] { "!addword", wordToAdd },
                isBroadcaster: true
            );

            var response = await _wordBlacklistService.HandleAddWordCommand(msgParams, wordType);

            Assert.That(response, Is.EqualTo("The word has been added to the blacklist!"));

            var addedWord = await _dbContext.Blacklist.FirstOrDefaultAsync(b =>
                b.Word == wordToAdd &&
                b.WordType == wordType &&
                b.Channel.BroadcasterTwitchChannelName == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);
            Assert.That(addedWord, Is.Not.Null);

            _wordBlacklistMonitorServiceMock.Verify(x => x.AddWordToBlacklist(
                wordToAdd,
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                wordType), Times.Once);
        }

        [Test]
        public async Task HandleAddWordCommand_UserIsSuperModAndCommandIsValid_AddsWordAndReturnsSuccessMessage()
        {
            var wordToAdd = "anotherbadword";
            var wordType = WordType.PermBanWord;

            _twitchHelperServiceMock
                .Setup(x => x.IsUserSuperModInChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1SuperModUserTwitchUserId))
                .ReturnsAsync(true);

            var msgParams = MessageParamsHelper.CreateChatMessageParams(
                $"!addword {wordToAdd}",
                "msg5",
                new string[] { "!addword", wordToAdd },
                isBroadcaster: false,
                chatterChannelId: DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
                chatterChannelName: DatabaseSeedHelper.Channel1SuperModUserTwitchUsername
            );

            var response = await _wordBlacklistService.HandleAddWordCommand(msgParams, wordType);

            Assert.That(response, Is.EqualTo("The word has been added to the blacklist!"));

            var addedWord = await _dbContext.Blacklist.FirstOrDefaultAsync(b =>
                b.Word == wordToAdd &&
                b.WordType == wordType &&
                b.Channel.BroadcasterTwitchChannelName == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);
            Assert.That(addedWord, Is.Not.Null);

            _wordBlacklistMonitorServiceMock.Verify(x => x.AddWordToBlacklist(
                wordToAdd,
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                wordType), Times.Once);
        }

        [Test]
        public async Task HandleAddWordCommand_AddsWordWithDifferentCasingAndWhitespace_StoresLowercaseTrimmedWord()
        {
            var inputWord = "  CaseSensitiveWORD ";
            var expectedStoredWord = "casesensitiveword";
            var wordType = WordType.PermBanWord;
            var msgParams = MessageParamsHelper.CreateChatMessageParams(
                $"!addword {inputWord}",
                "msg6",
                new string[] { "!addword", inputWord },
                isBroadcaster: true
            );

            var response = await _wordBlacklistService.HandleAddWordCommand(msgParams, wordType);

            Assert.That(response, Is.EqualTo("The word has been added to the blacklist!"));

            var addedWord = await _dbContext.Blacklist.FirstOrDefaultAsync(b =>
                b.Word == expectedStoredWord &&
                b.WordType == wordType &&
                b.Channel.BroadcasterTwitchChannelName == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);
            Assert.That(addedWord, Is.Not.Null);

            _wordBlacklistMonitorServiceMock.Verify(x => x.AddWordToBlacklist(
                expectedStoredWord,
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                wordType), Times.Once);
        }
    }
}