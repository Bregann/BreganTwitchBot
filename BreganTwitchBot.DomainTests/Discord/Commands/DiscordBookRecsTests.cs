using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.BookRecs;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord.Commands
{
    [TestFixture]
    public class DiscordBookRecsDataTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<IEnvironmentalSettingHelper> _mockEnvironmentalSettingHelper;
        private DiscordBookRecsData _discordBookRecsDataService;


        private const string TestItemBook = "The Hobbit";
        private const string TestItemBookMixed = "The Silmarillion";
        private const string TestItemAuthor = "J.R.R. Tolkien";
        private const string TestItemGenre = "Fantasy";

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("testdb_discordbookrecs")
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

            // Seed initial data
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            await _dbContext.SaveChangesAsync();

            _mockEnvironmentalSettingHelper = new Mock<IEnvironmentalSettingHelper>();
            _discordBookRecsDataService = new DiscordBookRecsData(_dbContext, _mockEnvironmentalSettingHelper.Object);
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
        public async Task AddNewLikedItem_UserAllowed_AddsSingleItemCorrectly()
        {
            await _discordBookRecsDataService.AddNewLikedItem(DatabaseSeedHelper.DiscordUserId1, TestItemBook, AiType.Book);

            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);
            var likedItem = await _dbContext.AiBookData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.Value == TestItemBook && x.AiType == AiType.Book);

            Assert.That(likedItem, Is.Not.Null);
            Assert.That(likedItem.Value, Is.EqualTo(TestItemBook));
        }

        [Test]
        public async Task AddNewLikedItem_UserAllowed_AddsMultipleItemsCorrectly()
        {
            string itemsToAdd = $"{TestItemBook},{TestItemAuthor}, {TestItemGenre}";
            await _discordBookRecsDataService.AddNewLikedItem(DatabaseSeedHelper.DiscordUserId1, itemsToAdd, AiType.Genre);

            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);
            var likedItems = await _dbContext.AiBookData.Where(x => x.ChannelUserId == user.Id && x.AiType == AiType.Genre).ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(likedItems, Has.Count.EqualTo(3));
                Assert.That(likedItems.Any(li => li.Value == TestItemBook), Is.True);
                Assert.That(likedItems.Any(li => li.Value == TestItemAuthor), Is.True);
                Assert.That(likedItems.Any(li => li.Value == TestItemGenre), Is.True);
            });
        }

        [Test]
        public async Task AddNewLikedItem_UserAllowed_AddsMixedCaseItem()
        {
            await _discordBookRecsDataService.AddNewLikedItem(DatabaseSeedHelper.DiscordUserId1, TestItemBookMixed, AiType.Book);

            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);
            var likedItem = await _dbContext.AiBookData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.Value == TestItemBookMixed);

            Assert.That(likedItem, Is.Not.Null);
            Assert.That(likedItem.Value, Is.EqualTo(TestItemBookMixed));
        }


        [Test]
        public void AddNewLikedItem_UserNotAllowed_ThrowsUnauthorizedAccessException()
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _discordBookRecsDataService.AddNewLikedItem(DatabaseSeedHelper.DiscordUserId2, TestItemBook, AiType.Book),
                "You are not allowed to use OpenAI features. Please contact the server administrator for assistance.");
        }

        [Test]
        public void AddNewLikedItem_UserNotFoundInChannelUsers_ThrowsUnauthorizedAccessException()
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                 await _discordBookRecsDataService.AddNewLikedItem(DatabaseSeedHelper.DiscordUserNonExistentId, TestItemBook, AiType.Book)
            );
        }

        [Test]
        public async Task AddNewLikedItem_UserAllowed_DifferentAiTypesAreStoredCorrectly()
        {
            await _discordBookRecsDataService.AddNewLikedItem(DatabaseSeedHelper.DiscordUserId1, TestItemBook, AiType.Book);
            await _discordBookRecsDataService.AddNewLikedItem(DatabaseSeedHelper.DiscordUserId1, TestItemAuthor, AiType.Author);

            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);
            var bookItem = await _dbContext.AiBookData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.Value == TestItemBook && x.AiType == AiType.Book);
            var authorItem = await _dbContext.AiBookData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.Value == TestItemAuthor && x.AiType == AiType.Author);

            Assert.Multiple(() =>
            {
                Assert.That(bookItem, Is.Not.Null);
                Assert.That(authorItem, Is.Not.Null);
            });
        }

        [Test]
        public async Task RemoveLikedItem_UserAllowed_RemovesExistingLowercaseItem()
        {
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);
            await _dbContext.AiBookData.AddAsync(new AiBookData { ChannelUserId = user.Id, Value = TestItemBook, AiType = AiType.Book });
            await _dbContext.SaveChangesAsync();

            await _discordBookRecsDataService.RemoveLikedItem(DatabaseSeedHelper.DiscordUserId1, TestItemBook);

            var likedItem = await _dbContext.AiBookData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.Value == TestItemBook);
            Assert.That(likedItem, Is.Null);
        }


        [Test]
        public async Task RemoveLikedItemWithUpperCaseName_UserAllowed_RemovesExistingLowercaseItem()
        {
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);
            await _dbContext.AiBookData.AddAsync(new AiBookData { ChannelUserId = user.Id, Value = TestItemBook, AiType = AiType.Book });
            await _dbContext.SaveChangesAsync();

            await _discordBookRecsDataService.RemoveLikedItem(DatabaseSeedHelper.DiscordUserId1, TestItemBook.ToUpper());

            var likedItem = await _dbContext.AiBookData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.Value == TestItemBook);
            Assert.That(likedItem, Is.Null);
        }

        [Test]
        public void RemoveLikedItem_UserAllowed_ItemNotFound_ThrowsInvalidOperationException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _discordBookRecsDataService.RemoveLikedItem(DatabaseSeedHelper.DiscordUserId1, "non existent book"),
                "No matching liked item found to remove.");
        }

        [Test]
        public void RemoveLikedItem_UserNotAllowed_ThrowsUnauthorizedAccessException()
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _discordBookRecsDataService.RemoveLikedItem(DatabaseSeedHelper.DiscordUserId2, TestItemBook),
                "You are not allowed to use OpenAI features. Please contact the server administrator for assistance.");
        }

        [Test]
        public void RemoveLikedItem_UserNotFoundInChannelUsers_ThrowsUnauthorizedAccessException()
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                 await _discordBookRecsDataService.RemoveLikedItem(DatabaseSeedHelper.DiscordUserNonExistentId, TestItemBook)
            );
        }

        [Test]
        public async Task RemoveLikedItem_ItemWithLeadingTrailingSpaces_RemovesCorrectly()
        {
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);

            await _dbContext.AiBookData.AddAsync(new AiBookData { ChannelUserId = user.Id, Value = TestItemAuthor, AiType = AiType.Author });
            await _dbContext.SaveChangesAsync();

            await _discordBookRecsDataService.RemoveLikedItem(DatabaseSeedHelper.DiscordUserId1, $"  {TestItemAuthor}  ");

            var likedItem = await _dbContext.AiBookData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.Value == TestItemAuthor);
            Assert.That(likedItem, Is.Null);
        }
    }
}