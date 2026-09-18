using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.Leaderboards;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class DiscordLeaderboardsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private DiscordLeaderboardsData _leaderboardsData;

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

            _leaderboardsData = new DiscordLeaderboardsData(_dbContext);
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
        public async Task PointsLeaderboard_IsOrderedDescending()
        {
            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.Points);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Select(x => x.Value), Is.Ordered.Descending);
        }

        [Test]
        public async Task Leaderboard_PositionsStartAtOneAndIncrement()
        {
            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.Points);

            Assert.That(result.Select(x => x.Position), Is.EqualTo(Enumerable.Range(1, result.Count)));
        }

        [Test]
        public async Task Leaderboard_RespectsTheTakeLimit()
        {
            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.Points, take: 1);

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task MonthlyHoursLeaderboard_UsesMonthlyWatchtime()
        {
            // the old bot ordered this by BitsDonatedThisMonth. Give the second user more monthly
            // watchtime and confirm they come top.
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var user2 = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User2TwitchUserId);

            var watchtime = await _dbContext.ChannelUserWatchtime.FirstAsync(x => x.ChannelUserId == user2.Id && x.ChannelId == channel.Id);
            watchtime.MinutesWatchedThisMonth = 99999;
            await _dbContext.SaveChangesAsync();

            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.MonthlyHours);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].Username, Is.EqualTo(DatabaseSeedHelper.Channel1User2TwitchUsername));
                Assert.That(result[0].Value, Is.EqualTo(99999));
            });
        }

        [Test]
        public async Task MarblesLeaderboard_ReturnsWins()
        {
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            var stats = await _dbContext.ChannelUserStats.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);
            stats.MarblesWins = 12;
            await _dbContext.SaveChangesAsync();

            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.Marbles);

            Assert.That(result[0].Value, Is.EqualTo(12));
        }

        [Test]
        public async Task AllTimeHoursLeaderboard_IsOrderedDescending()
        {
            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.AllTimeHours);

            Assert.That(result.Select(x => x.Value), Is.Ordered.Descending);
        }

        [Test]
        public async Task UnknownGuild_ReturnsEmpty()
        {
            var result = await _leaderboardsData.GetLeaderboardAsync(99999999, DiscordLeaderboardType.Points);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task Leaderboard_OnlyIncludesTheGuildsOwnChannel()
        {
            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.Points);

            // channel 2's user must not appear in channel 1's leaderboard
            Assert.That(result.Select(x => x.Username), Does.Not.Contain(DatabaseSeedHelper.Channel2User1TwitchUsername));
        }

        [Test]
        public async Task DiscordXpLeaderboard_WithNoStats_ReturnsEmpty()
        {
            // no DiscordUserStats rows are seeded, so this should be empty rather than throwing
            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.DiscordXp);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task DailyStreakLeaderboard_ReturnsDailyStreaks()
        {
            var result = await _leaderboardsData.GetLeaderboardAsync(DatabaseSeedHelper.DiscordGuildId, DiscordLeaderboardType.DailyStreak);

            Assert.That(result.Select(x => x.Value), Is.Ordered.Descending);
        }
    }
}
