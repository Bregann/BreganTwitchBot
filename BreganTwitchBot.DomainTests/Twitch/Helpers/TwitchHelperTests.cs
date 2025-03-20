using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch;
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
    }
}
