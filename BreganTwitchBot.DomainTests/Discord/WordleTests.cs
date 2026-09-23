using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.Wordle;
using WordleHelper = BreganTwitchBot.Domain.Services.Helpers.WordleHelper;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class WordleTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<IDiscordHelperService> _discordHelperService;

        private DiscordWordleData _wordleData;

        private const ulong LinkedUserId = DatabaseSeedHelper.DiscordUserId1;
        private const ulong UnlinkedUserId = DatabaseSeedHelper.DiscordUserNonExistentId;

        private static string Answer => WordleHelper.GetWordForDate(DateOnly.FromDateTime(DateTime.UtcNow));

        /// <summary>
        /// A valid word that isn't today's answer
        /// </summary>
        private static string WrongWord(int skip = 0) => WordleHelper.Answers.Where(x => x != Answer).Skip(skip).First();

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

            _discordHelperService = new Mock<IDiscordHelperService>();
            _wordleData = new DiscordWordleData(_dbContext, _discordHelperService.Object);
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
        public async Task CorrectGuess_SolvesAndAwardsPoints()
        {
            var result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, Answer);

            Assert.Multiple(() =>
            {
                Assert.That(result.Response, Does.Contain("1/6"));
                Assert.That(result.CanGuess, Is.False);
                Assert.That(result.PublicMessage, Is.Not.Null);
                Assert.That(result.PublicMessage, Does.Not.Contain(Answer.ToUpper()));
            });

            _discordHelperService.Verify(x => x.AddPointsToUser(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, WordleHelper.GetPointsForGuesses(1)), Times.Once());
        }

        [Test]
        public async Task WrongGuess_UsesAGuessAndShowsTheBoard()
        {
            var result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, WrongWord());

            Assert.Multiple(() =>
            {
                Assert.That(result.Response, Does.Contain("5 guesses left"));
                Assert.That(result.CanGuess, Is.True);
                Assert.That(result.PublicMessage, Is.Null);
            });

            var game = await _dbContext.DiscordWordleGames.SingleAsync();
            Assert.That(game.Guesses, Is.EqualTo(new[] { WrongWord() }));
        }

        [Test]
        public async Task GuessesAreCaseInsensitive()
        {
            var result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, $"  {Answer.ToUpper()} ");

            Assert.That(result.Response, Does.Contain("You got it"));
        }

        [Test]
        public async Task InvalidGuess_DoesNotUseAGuess()
        {
            var result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, "toolong");

            Assert.Multiple(async () =>
            {
                Assert.That(result.Response, Does.Contain("5 letters"));
                Assert.That(result.CanGuess, Is.True);
                Assert.That(await _dbContext.DiscordWordleGames.AnyAsync(), Is.False);
            });
        }

        [Test]
        public async Task RepeatedGuess_DoesNotUseAGuess()
        {
            await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, WrongWord());
            var result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, WrongWord());

            Assert.That(result.Response, Does.Contain("already guessed"));

            var game = await _dbContext.DiscordWordleGames.SingleAsync();
            Assert.That(game.Guesses, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task SixWrongGuesses_EndsTheGameAndRevealsTheWord()
        {
            WordleResponse result = null!;

            for (var i = 0; i < WordleHelper.MaxGuesses; i++)
            {
                result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, WrongWord(i));
            }

            Assert.Multiple(() =>
            {
                Assert.That(result.Response, Does.Contain(Answer.ToUpper()));
                Assert.That(result.CanGuess, Is.False);
                Assert.That(result.PublicMessage, Does.Contain("X/6"));
            });

            _discordHelperService.Verify(x => x.AddPointsToUser(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<long>()), Times.Never());
        }

        [Test]
        public async Task FinishedGame_CannotBePlayedAgainToday()
        {
            await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, Answer);
            var result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, Answer);

            Assert.Multiple(() =>
            {
                Assert.That(result.Response, Does.Contain("already finished"));
                Assert.That(result.CanGuess, Is.False);
                Assert.That(result.PublicMessage, Is.Null);
            });

            // points only once
            _discordHelperService.Verify(x => x.AddPointsToUser(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<long>()), Times.Once());
        }

        [Test]
        public async Task UnlinkedUser_CanPlayButEarnsNoPoints()
        {
            var result = await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, UnlinkedUserId, Answer);

            Assert.That(result.Response, Does.Contain("Link your Twitch account"));
            _discordHelperService.Verify(x => x.AddPointsToUser(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<long>()), Times.Never());
        }

        [Test]
        public async Task EachUserHasTheirOwnBoard()
        {
            await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, WrongWord());

            var board = await _wordleData.GetBoard(DatabaseSeedHelper.DiscordGuildId, DatabaseSeedHelper.DiscordUserId2);

            Assert.Multiple(() =>
            {
                Assert.That(board.Response, Does.Contain("haven't guessed yet"));
                Assert.That(board.CanGuess, Is.True);
            });
        }

        [Test]
        public async Task GetBoard_ShowsPreviousGuesses()
        {
            await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, WrongWord());

            var board = await _wordleData.GetBoard(DatabaseSeedHelper.DiscordGuildId, LinkedUserId);

            Assert.That(board.Response, Does.Contain(WrongWord().ToUpper()));
        }

        [Test]
        public async Task GetBoard_AfterFinishing_DoesNotOfferAnotherGuess()
        {
            await _wordleData.Guess(DatabaseSeedHelper.DiscordGuildId, LinkedUserId, Answer);

            var board = await _wordleData.GetBoard(DatabaseSeedHelper.DiscordGuildId, LinkedUserId);

            Assert.Multiple(() =>
            {
                Assert.That(board.Response, Does.Contain("Come back tomorrow"));
                Assert.That(board.CanGuess, Is.False);
            });
        }

        [Test]
        public async Task UnlinkedServer_IsRejected()
        {
            var result = await _wordleData.Guess(424242, LinkedUserId, Answer);

            Assert.That(result.Response, Does.Contain("isn't linked"));
        }
    }
}
