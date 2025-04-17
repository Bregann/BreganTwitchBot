using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Helpers
{
    [TestFixture]
    public class TwitchHelperTests
    {
        private PostgreSqlContainer _postgresContainer;
        private ServiceProvider _serviceProvider;
        private AppDbContext _dbContext;
        private TwitchHelperService _twitchHelperService;

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
            var services = new ServiceCollection();

            // Register DbContext with PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseLazyLoadingProxies()
                       .UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped
            );

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Resolve DbContext and set up database
            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            // Initialize TwitchHelperService with real IServiceProvider
            var mockApiConnection = new Mock<ITwitchApiConnection>();
            var mockApiInteractionService = new Mock<ITwitchApiInteractionService>();

            _twitchHelperService = new TwitchHelperService(
                mockApiConnection.Object,
                _serviceProvider,  // Use real DI container
                mockApiInteractionService.Object
            );
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

        [Test]
        [TestCase(DatabaseSeedHelper.Channel1User1TwitchUsername, DatabaseSeedHelper.Channel1User1TwitchUserId)]
        [TestCase(DatabaseSeedHelper.Channel1User1TwitchUsername + "    ", DatabaseSeedHelper.Channel1User1TwitchUserId)]
        [TestCase("COOLUSER", DatabaseSeedHelper.Channel1User1TwitchUserId)]
        [TestCase(DatabaseSeedHelper.Channel1User2TwitchUsername, DatabaseSeedHelper.Channel1User2TwitchUserId)]
        [TestCase(DatabaseSeedHelper.Channel2User1TwitchUsername, DatabaseSeedHelper.Channel2User1TwitchUserId)]
        public async Task GetTwitchUserIdFromUsername_WhenUserExists_ReturnsUserId(string username, string expectedUserId)
        {
            var result = await _twitchHelperService.GetTwitchUserIdFromUsername(username);
            Assert.That(result, Is.EqualTo(expectedUserId));
        }

        [Test]
        public async Task GetTwitchUserIdFromUsername_WhenUserDoesNotExist_ReturnsNull()
        {
            var result = await _twitchHelperService.GetTwitchUserIdFromUsername("nonexistentuser");
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1ChannelCurrencyName)]
        [TestCase(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId + "     ", DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1ChannelCurrencyName)]
        public async Task GetPointsName_WhenBroadcasterExists_ReturnsPointsName(string broadcasterChannelId, string broadcasterChannelName, string expectedPointsName)
        {
            var result = await _twitchHelperService.GetPointsName(broadcasterChannelId, broadcasterChannelName);
            Assert.That(result, Is.EqualTo(expectedPointsName));
        }

        [Test]
        public async Task IsUserSuperModInChannel_WhenUserIsSuperMod_ReturnsTrue()
        {
            var broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var superModUserId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId;

            var result = await _twitchHelperService.IsUserSuperModInChannel(broadcasterChannelId, superModUserId);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsUserSuperModInChannel_WhenUserIsNotSuperMod_ReturnsFalse()
        {
            var broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var nonSuperModUserId = DatabaseSeedHelper.Channel1User1TwitchUserId;

            var result = await _twitchHelperService.IsUserSuperModInChannel(broadcasterChannelId, nonSuperModUserId);
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsUserSuperModInChannel_WhenUserDoesNotExist_ReturnsFalse()
        {
            var broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var nonExistentUserId = "nonexistentuser";
            var result = await _twitchHelperService.IsUserSuperModInChannel(broadcasterChannelId, nonExistentUserId);
            Assert.That(result, Is.False);
        }

        [Test]
        public void EnsureUserHasModeratorPermissions_WhenUserIsBroadcaster_DoesNotThrowException()
        {
            var isMod = true;
            var isBroadcaster = true;
            var viewerUsername = "coolstreamername";
            var viewerChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var broadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName;

            Assert.DoesNotThrowAsync(async () =>
                await _twitchHelperService.EnsureUserHasModeratorPermissions(isMod, isBroadcaster, viewerUsername, viewerChannelId, broadcasterChannelId, broadcasterChannelName)
            );
        }

        [Test]
        public void EnsureUserHasModeratorPermissions_WhenUserIsSuperMod_DoesNotThrowException()
        {
            var isMod = true;
            var isBroadcaster = false;
            var viewerUsername = "supermoduser";
            var viewerChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId;
            var broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var broadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName;

            Assert.DoesNotThrowAsync(async () =>
                await _twitchHelperService.EnsureUserHasModeratorPermissions(isMod, isBroadcaster, viewerUsername, viewerChannelId, broadcasterChannelId, broadcasterChannelName)
            );
        }

        [Test]
        public void EnsureUserHasModeratorPermissions_WhenUserIsMod_DoesNotThrowException()
        {
            var isMod = true;
            var isBroadcaster = false;
            var viewerUsername = "cooluser";
            var viewerChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId;
            var broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var broadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName;

            Assert.DoesNotThrowAsync(async () =>
                await _twitchHelperService.EnsureUserHasModeratorPermissions(isMod, isBroadcaster, viewerUsername, viewerChannelId, broadcasterChannelId, broadcasterChannelName)
            );
        }

        [Test]
        public void EnsureUserHasModeratorPermissions_WhenUserIsNotMod_ThrowsUnauthorizedAccessException()
        {
            var isMod = false;
            var isBroadcaster = false;
            var viewerUsername = "cooluser";
            var viewerChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId;
            var broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
            var broadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName;

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _twitchHelperService.EnsureUserHasModeratorPermissions(isMod, isBroadcaster, viewerUsername, viewerChannelId, broadcasterChannelId, broadcasterChannelName)
            );
        }

        [Test]
        public async Task AddPointsToUser_AddPointsWhenUserExists_AddsPoints()
        {
            var pointsToAdd = 100;
            _dbContext.ChangeTracker.Clear();

            await _twitchHelperService.AddPointsToUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, pointsToAdd, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1User1TwitchUsername);

            var user = await _dbContext.ChannelUserData
                .FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.That(user.Points, Is.EqualTo(200));
        }

        [Test]
        public void AddPointsToUser_WhenUserDoesNotExist_ThrowsTwitchUserNotFoundException()
        {
            var pointsToAdd = 100;
            Assert.ThrowsAsync<TwitchUserNotFoundException>(async () =>
                await _twitchHelperService.AddPointsToUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, "nonexistentuser", pointsToAdd, "DoesntExist", DatabaseSeedHelper.Channel1User1TwitchUsername)
            );
        }

        [Test]
        public void AddPointsToUser_WhenUserExistsAndPointsToAddIsNegative_ThrowsInvalidOperationException()
        {
            var pointsToAdd = -100;
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _twitchHelperService.AddPointsToUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, pointsToAdd, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1User1TwitchUsername)
            );
        }

        [Test]
        public async Task AddPointsToUser_AddOverPointsCapToUser_AddsMaxOfPointsCap()
        {
            var pointsToAdd = 9999999;
            _dbContext.ChangeTracker.Clear();

            await _twitchHelperService.AddPointsToUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, pointsToAdd, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1User1TwitchUsername);

            var user = await _dbContext.ChannelUserData
                .FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.That(user.Points, Is.EqualTo(1000));
        }

        [Test]
        public async Task RemovePointsFromUser_RemovePointsWhenUserExists_RemovesPoints()
        {
            var pointsToRemove = 50;
            _dbContext.ChangeTracker.Clear();

            await _twitchHelperService.RemovePointsFromUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, pointsToRemove, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1User1TwitchUsername);

            var user = await _dbContext.ChannelUserData
                .FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.That(user.Points, Is.EqualTo(50));
        }

        [Test]
        public void RemovePointsFromUser_WhenUserDoesNotExist_ThrowsTwitchUserNotFoundException()
        {
            var pointsToRemove = 50;
            Assert.ThrowsAsync<TwitchUserNotFoundException>(async () =>
                await _twitchHelperService.RemovePointsFromUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, "nonexistentuser", pointsToRemove, "DoesntExist", DatabaseSeedHelper.Channel1User1TwitchUsername)
            );
        }

        [Test]
        public void RemovePointsFromUser_WhenUserExistsAndPointsToRemoveIsNegative_ThrowsInvalidOperationException()
        {
            var pointsToRemove = -50;
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _twitchHelperService.RemovePointsFromUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, pointsToRemove, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1User1TwitchUsername)
            );
        }

        [Test]
        public async Task RemovePointsFromUser_RemoveOverPointsCapFromUser_RemovesAllPoints()
        {
            var pointsToRemove = 9999999;
            _dbContext.ChangeTracker.Clear();

            await _twitchHelperService.RemovePointsFromUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, pointsToRemove, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1User1TwitchUsername);

            var user = await _dbContext.ChannelUserData
                .FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId && x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.That(user.Points, Is.EqualTo(0));
        }

        [Test]
        public async Task GetPointsForUser_WhenUserExists_ReturnsPoints()
        {
            var result = await _twitchHelperService.GetPointsForUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DatabaseSeedHelper.Channel1User1TwitchUsername);
            Assert.That(result, Is.EqualTo(100));
        }

        [Test]
        public void GetPointsForUser_WhenUserDoesNotExist_ThrowsTwitchUserNotFoundException()
        {
            Assert.ThrowsAsync<TwitchUserNotFoundException>(async () =>
                await _twitchHelperService.GetPointsForUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, "nonexistentuser", DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, "DoesntExist")
            );
        }

        [Test]
        public async Task AddOrUpdateUserToDatabase_ShouldAddNewUser_WhenUserDoesNotExist()
        {
            await _twitchHelperService.AddOrUpdateUserToDatabase(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                "99999", // New user ID
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                "newuser"
            );

            var user = await _dbContext.ChannelUsers.FirstOrDefaultAsync(u => u.TwitchUserId == "99999");

            Assert.That(user, Is.Not.Null);
            Assert.That(user.TwitchUsername, Is.EqualTo("newuser"));
        }

        [Test]
        public async Task AddOrUpdateUserToDatabase_ShouldUpdateExistingUser_WhenUserExists()
        {
            await _twitchHelperService.AddOrUpdateUserToDatabase(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                DatabaseSeedHelper.Channel1User1TwitchUserId,
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                "updateduser"
            );

            var user = await _dbContext.ChannelUsers.FirstAsync(u => u.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            await _dbContext.Entry(user).ReloadAsync();

            Assert.That(user, Is.Not.Null);
            Assert.That(user.TwitchUsername, Is.EqualTo("updateduser"));
        }

        [Test]
        public async Task AddOrUpdateUserToDatabase_ShouldNotDuplicateUserStats_WhenUserAlreadyHasStats()
        {
            await _twitchHelperService.AddOrUpdateUserToDatabase(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                DatabaseSeedHelper.Channel1User1TwitchUserId,
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                DatabaseSeedHelper.Channel1User1TwitchUsername
            );

            var user = await _dbContext.ChannelUsers.Include(u => u.ChannelUserStats)
                .FirstOrDefaultAsync(u => u.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.That(user, Is.Not.Null);
            Assert.That(user.ChannelUserStats, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task AddOrUpdateUserToDatabase_ShouldAddWatchtime_WhenAddMinutesIsTrue()
        {
            await _twitchHelperService.AddOrUpdateUserToDatabase(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                DatabaseSeedHelper.Channel2User1TwitchUserId,
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                DatabaseSeedHelper.Channel2User1TwitchUsername,
                addMinutes: true
            );

            var user = await _dbContext.ChannelUsers.Include(u => u.ChannelUserWatchtimes)
                .FirstOrDefaultAsync(u => u.TwitchUserId == DatabaseSeedHelper.Channel2User1TwitchUserId);

            Assert.That(user, Is.Not.Null);
            Assert.That(user.ChannelUserWatchtimes.First().MinutesWatchedThisStream, Is.EqualTo(1));
        }

    }
}
