using BreganTwitchBot.Domain.Services.Twitch.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Events
{
    [TestFixture]
    public class WordBlacklistMonitorServiceTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private IServiceProvider _serviceProvider;
        private Mock<ITwitchHelperService> _twitchHelperServiceMock;
        private WordBlacklistMonitorService _monitorService;

        // Assume DatabaseSeedHelper adds these words:

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

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(provider => _dbContext);

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _twitchHelperServiceMock = new Mock<ITwitchHelperService>();

            _monitorService = new WordBlacklistMonitorService(_serviceProvider, _twitchHelperServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _postgresContainer.DisposeAsync();
        }

        [Test]
        public void IsWordBlacklisted_SeededBannedWordForChannel1_ReturnsTrueAndBannedType()
        {
            var word = DatabaseSeedHelper.SeededChannel1BannedWord;
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            _monitorService.AddWordToBlacklist(word, broadcasterId, WordType.PermBanWord);

            var result = _monitorService.IsWordBlacklisted(word, broadcasterId);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsBlacklisted, Is.True);
                Assert.That(result.BlacklistType, Is.EqualTo(WordType.PermBanWord));
            });
        }

        [Test]
        public void IsWordBlacklisted_SeededTempBanWordForChannel1_ReturnsTrueAndTempBanType()
        {
            var word = DatabaseSeedHelper.SeededChannel1TempBanWord;
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            _monitorService.AddWordToBlacklist(word, broadcasterId, WordType.TempBanWord);

            var result = _monitorService.IsWordBlacklisted(word, broadcasterId);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsBlacklisted, Is.True);
                Assert.That(result.BlacklistType, Is.EqualTo(WordType.TempBanWord));
            });
        }

        [Test]
        public void IsWordBlacklisted_SeededBannedWordForChannel2_ReturnsTrueAndBannedType()
        {
            var word = DatabaseSeedHelper.SeededChannel2BannedWord;
            var broadcasterId = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId;
            _monitorService.AddWordToBlacklist(word, broadcasterId, WordType.PermBanWord);

            var result = _monitorService.IsWordBlacklisted(word, broadcasterId);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsBlacklisted, Is.True);
                Assert.That(result.BlacklistType, Is.EqualTo(WordType.PermBanWord));
            });
        }

        [Test]
        public void IsWordBlacklisted_WordExistsForDifferentBroadcaster_ReturnsFalse()
        {
            var word = DatabaseSeedHelper.SeededChannel1BannedWord;
            var broadcasterId = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId;

            var result = _monitorService.IsWordBlacklisted(word, broadcasterId);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsBlacklisted, Is.False);
                Assert.That(result.BlacklistType, Is.Null);
            });
        }

        [Test]
        public void IsWordBlacklisted_WordDoesNotExist_ReturnsFalse()
        {
            var word = "nonexistentword";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;

            var result = _monitorService.IsWordBlacklisted(word, broadcasterId);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsBlacklisted, Is.False);
                Assert.That(result.BlacklistType, Is.Null);
            });
        }

        [Test]
        public void AddWordToBlacklist_AddNewWord_WordIsAddedToInMemoryList()
        {
            var word = DatabaseSeedHelper.SeededChannel1BannedWord + "new";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var wordType = WordType.PermBanWord;
            var initialCount = _monitorService._wordBlacklist.Count;

            Assert.That(_monitorService._wordBlacklist.Any(i => i.Word == word && i.BroadcasterId == broadcasterId), Is.False);

            _monitorService.AddWordToBlacklist(word, broadcasterId, wordType);

            Assert.That(_monitorService._wordBlacklist, Has.Count.EqualTo(initialCount + 1));
            var addedItem = _monitorService._wordBlacklist.FirstOrDefault(i => i.Word == word && i.BroadcasterId == broadcasterId);

            Assert.That(addedItem, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(addedItem.Word, Is.EqualTo(word));
                Assert.That(addedItem.BroadcasterId, Is.EqualTo(broadcasterId));
                Assert.That(addedItem.WordType, Is.EqualTo(wordType));
            });

            Assert.That(_monitorService._wordBlacklist.Any(i => i.Word == word && i.BroadcasterId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId), Is.True);
        }

        [Test]
        public void RemoveWordFromBlacklist_RemoveSeededWord_WordIsRemovedFromInMemoryList()
        {
            var wordToRemove = DatabaseSeedHelper.SeededChannel1BannedWord;
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;

            _monitorService.AddWordToBlacklist(DatabaseSeedHelper.SeededChannel1TempBanWord, broadcasterId, WordType.TempBanWord);
            _monitorService.AddWordToBlacklist(DatabaseSeedHelper.SeededChannel2BannedWord, DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId, WordType.PermBanWord);

            var initialCount = _monitorService._wordBlacklist.Count;

            Assert.That(_monitorService._wordBlacklist.Any(item => item.Word == wordToRemove && item.BroadcasterId == broadcasterId), Is.True);

            _monitorService.RemoveWordFromBlacklist(wordToRemove, broadcasterId);

            Assert.Multiple(() =>
            {
                Assert.That(_monitorService._wordBlacklist, Has.Count.EqualTo(initialCount - 1));
                Assert.That(_monitorService._wordBlacklist.Any(item => item.Word == wordToRemove && item.BroadcasterId == broadcasterId), Is.False);

                Assert.That(_monitorService._wordBlacklist.Any(item => item.Word == DatabaseSeedHelper.SeededChannel1TempBanWord && item.BroadcasterId == broadcasterId), Is.True);
                Assert.That(_monitorService._wordBlacklist.Any(item => item.Word == DatabaseSeedHelper.SeededChannel2BannedWord && item.BroadcasterId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId), Is.True);
            });
        }

        [Test]
        public void RemoveWordFromBlacklist_AttemptToRemoveNonExistentWord_ListRemainsUnchanged()
        {
            var wordToRemove = "notthere";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var initialCount = _monitorService._wordBlacklist.Count;
            var initialListStateJson = System.Text.Json.JsonSerializer.Serialize(_monitorService._wordBlacklist);

            _monitorService.RemoveWordFromBlacklist(wordToRemove, broadcasterId);

            Assert.That(_monitorService._wordBlacklist, Has.Count.EqualTo(initialCount));
            var currentListStateJson = System.Text.Json.JsonSerializer.Serialize(_monitorService._wordBlacklist);
            Assert.That(currentListStateJson, Is.EqualTo(initialListStateJson));
        }

        [Test]
        public void RemoveWordFromBlacklist_AttemptToRemoveWordForDifferentBroadcaster_ListRemainsUnchanged()
        {
            var wordToRemove = DatabaseSeedHelper.SeededChannel1BannedWord;
            var targetBroadcasterId = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId;
            var actualBroadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;

            _monitorService.AddWordToBlacklist(wordToRemove, actualBroadcasterId, WordType.PermBanWord);

            var initialCount = _monitorService._wordBlacklist.Count;
            var initialListStateJson = System.Text.Json.JsonSerializer.Serialize(_monitorService._wordBlacklist);

            Assert.That(_monitorService._wordBlacklist.Any(item => item.Word == wordToRemove && item.BroadcasterId == actualBroadcasterId), Is.True);

            _monitorService.RemoveWordFromBlacklist(wordToRemove, targetBroadcasterId);

            Assert.That(_monitorService._wordBlacklist, Has.Count.EqualTo(initialCount));
            Assert.That(_monitorService._wordBlacklist.Any(item => item.Word == wordToRemove && item.BroadcasterId == actualBroadcasterId), Is.True);

            var currentListStateJson = System.Text.Json.JsonSerializer.Serialize(_monitorService._wordBlacklist);
            Assert.That(currentListStateJson, Is.EqualTo(initialListStateJson));
        }

        [Test]
        public void RemoveWarnedUsers_NoWarnedUsers_ListRemainsUnchanged()
        {
            var initialCount = _monitorService._wordBlacklistUsers.Count;
            _monitorService.RemoveWarnedUsers();
            Assert.That(_monitorService._wordBlacklistUsers, Has.Count.EqualTo(initialCount));
        }

        [Test]
        public void RemoveWarnedUsers_OneWarnedUser_ListIsCleared()
        {
            var userId = "12345";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            _monitorService._wordBlacklistUsers.Add(new WordBlacklistUser
            {
                UserId = userId,
                BroadcasterId = broadcasterId,
                AddedAt = DateTime.UtcNow.AddMinutes(-10)
            });

            _monitorService.RemoveWarnedUsers();
            Assert.That(_monitorService._wordBlacklistUsers, Has.Count.EqualTo(0));
        }

        [Test]
        public void RemoveWarnedUsers_NewWarnedUser_ListIsNotCleared()
        {
            var userId = "12345";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            _monitorService._wordBlacklistUsers.Add(new WordBlacklistUser
            {
                UserId = userId,
                BroadcasterId = broadcasterId,
                AddedAt = DateTime.UtcNow
            });

            _monitorService.RemoveWarnedUsers();
            Assert.That(_monitorService._wordBlacklistUsers, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task CheckMessageForBlacklistedWords_ContainsBlacklistedWord_TimesOutUser()
        {
            var message = "This is a test message with a blacklisted word. seededtempbanword";
            var userId = "12345";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var blacklistedWord = DatabaseSeedHelper.SeededChannel1TempBanWord;

            _monitorService.AddWordToBlacklist(blacklistedWord, broadcasterId, WordType.TempBanWord);

            await _monitorService.CheckMessageForBlacklistedWords(message, userId, broadcasterId);

            _twitchHelperServiceMock.Verify(x => x.TimeoutUser(broadcasterId, userId, 300, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CheckMessageForBlacklistedWords_ContainsBlacklistedWordAndUserAlreadyWarned_TimesOutUser()
        {
            var message = "This is a test message with a blacklisted word. seededtempbanword";
            var userId = "12345";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var blacklistedWord = DatabaseSeedHelper.SeededChannel1TempBanWord;

            _monitorService.AddWordToBlacklist(blacklistedWord, broadcasterId, WordType.StrikeWord);
            _monitorService._wordBlacklistUsers.Add(new WordBlacklistUser
            {
                UserId = userId,
                BroadcasterId = broadcasterId,
                AddedAt = DateTime.UtcNow
            });

            await _monitorService.CheckMessageForBlacklistedWords(message, userId, broadcasterId);

            _twitchHelperServiceMock.Verify(x => x.TimeoutUser(broadcasterId, userId, 300, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CheckMessageForBlacklistedWords_ContainsBlacklistedWordAndUserNotWarned_WarnsUser()
        {
            var message = "This is a test message with a blacklisted word. seededbannedword";
            var userId = "12345";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var blacklistedWord = DatabaseSeedHelper.SeededChannel1BannedWord;

            _monitorService.AddWordToBlacklist(blacklistedWord, broadcasterId, WordType.StrikeWord);

            await _monitorService.CheckMessageForBlacklistedWords(message, userId, broadcasterId);

            _twitchHelperServiceMock.Verify(x => x.WarnUser(broadcasterId, userId, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CheckMessageForBlacklistedWords_ContainsMultipleBlacklistedWords_TimesOutUser()
        {
            var message = "This is a test message with a blacklisted word and another blacklisted word. seededtempbanword seededbannedword";
            var userId = "12345";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var blacklistedWord1 = DatabaseSeedHelper.SeededChannel1BannedWord;
            var blacklistedWord2 = DatabaseSeedHelper.SeededChannel1TempBanWord;

            _monitorService.AddWordToBlacklist(blacklistedWord1, broadcasterId, WordType.PermBanWord);
            _monitorService.AddWordToBlacklist(blacklistedWord2, broadcasterId, WordType.TempBanWord);

            await _monitorService.CheckMessageForBlacklistedWords(message, userId, broadcasterId);

            _twitchHelperServiceMock.Verify(x => x.TimeoutUser(broadcasterId, userId, 300, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CheckMessageForBlacklistedWords_NoBlacklistedWords_NoActionTaken()
        {
            var message = "This is a test message without any blacklisted words.";
            var userId = "12345";
            var broadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;

            await _monitorService.CheckMessageForBlacklistedWords(message, userId, broadcasterId);

            _twitchHelperServiceMock.Verify(x => x.TimeoutUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _twitchHelperServiceMock.Verify(x => x.WarnUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}