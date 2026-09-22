using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Services.Api;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.Leaderboards;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Api
{
    [TestFixture]
    public class ApiDataServiceTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private ApiDataService _apiDataService;

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

            _apiDataService = new ApiDataService(_dbContext, new DiscordLeaderboardsData(_dbContext));
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
        public async Task GetLeaderboard_ReturnsPositionsForTheChannel()
        {
            var result = await _apiDataService.GetLeaderboardAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, DiscordLeaderboardType.Points);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.BroadcasterChannelName, Is.EqualTo(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName));
                Assert.That(result.LeaderboardName, Is.EqualTo("Points"));
            });
        }

        [Test]
        public async Task GetLeaderboard_ChannelNameIsCaseInsensitive()
        {
            var result = await _apiDataService.GetLeaderboardAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName.ToUpper(), DiscordLeaderboardType.Points);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetLeaderboard_UnknownChannel_ReturnsNull()
        {
            var result = await _apiDataService.GetLeaderboardAsync("notachannel", DiscordLeaderboardType.Points);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetCustomCommands_ReturnsTheChannelsCommands()
        {
            var result = await _apiDataService.GetCustomCommandsAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.Empty);
            });
        }

        [Test]
        public async Task GetCustomCommands_UnknownChannel_ReturnsNull()
        {
            var result = await _apiDataService.GetCustomCommandsAsync("notachannel");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetSubathonStatus_NotRunning_ReportsInactive()
        {
            var result = await _apiDataService.GetSubathonStatusAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Active, Is.False);
            });
        }

        [Test]
        public async Task GetSubathonStatus_Running_ReportsSecondsLeft()
        {
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            config.SubathonActive = true;
            config.SubathonTime = TimeSpan.FromHours(2);
            config.SubathonStartTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var result = await _apiDataService.GetSubathonStatusAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.Multiple(() =>
            {
                Assert.That(result!.Active, Is.True);
                Assert.That(result.SecondsLeft, Is.GreaterThan(7100));
                Assert.That(result.EndsAt, Is.Not.Null);
            });
        }

        [Test]
        public async Task GetSubathonStatus_TimeAlreadyUp_ReportsZeroRatherThanNegative()
        {
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            config.SubathonActive = true;
            config.SubathonTime = TimeSpan.FromHours(1);
            config.SubathonStartTime = DateTime.UtcNow.AddHours(-5);
            await _dbContext.SaveChangesAsync();

            var result = await _apiDataService.GetSubathonStatusAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.That(result!.SecondsLeft, Is.EqualTo(0));
        }

        [Test]
        public async Task GetSubathonStatus_UnknownChannel_ReturnsNull()
        {
            var result = await _apiDataService.GetSubathonStatusAsync("notachannel");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetSubathonLeaderboard_ReturnsContributorsInOrder()
        {
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var user1 = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            var user2 = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User2TwitchUserId);

            await _dbContext.Subathons.AddRangeAsync(
                new Domain.Database.Models.Subathon { ChannelId = channel.Id, ChannelUserId = user1.Id, BitsDonated = 100, SubsGifted = 1, TimeAdded = TimeSpan.FromMinutes(5) },
                new Domain.Database.Models.Subathon { ChannelId = channel.Id, ChannelUserId = user2.Id, BitsDonated = 5000, SubsGifted = 0, TimeAdded = TimeSpan.FromMinutes(50) });
            await _dbContext.SaveChangesAsync();

            var result = await _apiDataService.GetSubathonLeaderboardAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.Multiple(() =>
            {
                Assert.That(result!.TopBitsDonators[0].Username, Is.EqualTo(DatabaseSeedHelper.Channel1User2TwitchUsername));
                Assert.That(result.TopBitsDonators[0].Amount, Is.EqualTo(5000));
                Assert.That(result.TopBitsDonators[0].Position, Is.EqualTo(1));

                // only the user who actually gifted appears in the sub gifters list
                Assert.That(result.TopSubGifters, Has.Count.EqualTo(1));
                Assert.That(result.TopSubGifters[0].Username, Is.EqualTo(DatabaseSeedHelper.Channel1User1TwitchUsername));
            });
        }

        [Test]
        public async Task GetSubathonLeaderboard_UnknownChannel_ReturnsNull()
        {
            var result = await _apiDataService.GetSubathonLeaderboardAsync("notachannel");

            Assert.That(result, Is.Null);
        }
    }
}
