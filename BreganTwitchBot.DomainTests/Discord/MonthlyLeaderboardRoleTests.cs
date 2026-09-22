using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    /// <summary>
    /// The role assignment itself needs a live Discord guild, so these cover the data side:
    /// the monthly totals that decide who leads, and the configured roles that drive it.
    /// </summary>
    [TestFixture]
    public class MonthlyLeaderboardRoleTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

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

        private async Task<Channel> GetChannel()
        {
            return await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
        }

        [Test]
        public async Task MonthlyLeaderboardRoles_CanBeConfiguredPerChannelAndPosition()
        {
            var channel = await GetChannel();

            await _dbContext.MonthlyLeaderboardRoles.AddRangeAsync(
                new MonthlyLeaderboardRole { ChannelId = channel.Id, LeaderboardType = MonthlyLeaderboardType.BitsDonated, Position = 1, DiscordRoleId = 111 },
                new MonthlyLeaderboardRole { ChannelId = channel.Id, LeaderboardType = MonthlyLeaderboardType.BitsDonated, Position = 2, DiscordRoleId = 222 },
                new MonthlyLeaderboardRole { ChannelId = channel.Id, LeaderboardType = MonthlyLeaderboardType.SubsGifted, Position = 1, DiscordRoleId = 333 });
            await _dbContext.SaveChangesAsync();

            var bitsRoles = await _dbContext.MonthlyLeaderboardRoles
                .Where(x => x.ChannelId == channel.Id && x.LeaderboardType == MonthlyLeaderboardType.BitsDonated)
                .OrderBy(x => x.Position)
                .ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(bitsRoles, Has.Count.EqualTo(2));
                Assert.That(bitsRoles[0].DiscordRoleId, Is.EqualTo(111));
                Assert.That(bitsRoles[1].DiscordRoleId, Is.EqualTo(222));
            });
        }

        [Test]
        public async Task MonthlyTotals_OrderLeadersCorrectly()
        {
            var channel = await GetChannel();
            var user1 = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            var user2 = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User2TwitchUserId);

            var stats1 = await _dbContext.ChannelUserStats.FirstAsync(x => x.ChannelUserId == user1.Id && x.ChannelId == channel.Id);
            var stats2 = await _dbContext.ChannelUserStats.FirstAsync(x => x.ChannelUserId == user2.Id && x.ChannelId == channel.Id);

            stats1.BitsDonatedThisMonth = 100;
            stats2.BitsDonatedThisMonth = 5000;
            await _dbContext.SaveChangesAsync();

            var leaders = await _dbContext.ChannelUserStats
                .Where(x => x.ChannelId == channel.Id && x.BitsDonatedThisMonth > 0)
                .OrderByDescending(x => x.BitsDonatedThisMonth)
                .Select(x => x.User.TwitchUsername)
                .ToListAsync();

            Assert.That(leaders[0], Is.EqualTo(DatabaseSeedHelper.Channel1User2TwitchUsername));
        }

        [Test]
        public async Task MonthlyTotals_AreTrackedPerChannel()
        {
            var channel1 = await GetChannel();
            var user1 = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            var stats = await _dbContext.ChannelUserStats.FirstAsync(x => x.ChannelUserId == user1.Id && x.ChannelId == channel1.Id);
            stats.BitsDonatedThisMonth = 999;
            await _dbContext.SaveChangesAsync();

            var channel2 = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);

            var channel2Total = await _dbContext.ChannelUserStats
                .Where(x => x.ChannelId == channel2.Id)
                .SumAsync(x => x.BitsDonatedThisMonth);

            Assert.That(channel2Total, Is.EqualTo(0));
        }

        [Test]
        public async Task ResettingMonthlyTotals_OnlyClearsTheMonthlyColumns()
        {
            var channel = await GetChannel();
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            var stats = await _dbContext.ChannelUserStats.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);
            stats.BitsDonatedThisMonth = 500;
            stats.GiftedSubsThisMonth = 5;
            var bossesDoneBefore = stats.BossesDone;
            await _dbContext.SaveChangesAsync();

            // what the first-of-month reset does
            stats.BitsDonatedThisMonth = 0;
            stats.GiftedSubsThisMonth = 0;
            await _dbContext.SaveChangesAsync();

            var reloaded = await _dbContext.ChannelUserStats.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            Assert.Multiple(() =>
            {
                Assert.That(reloaded.BitsDonatedThisMonth, Is.EqualTo(0));
                Assert.That(reloaded.GiftedSubsThisMonth, Is.EqualTo(0));
                Assert.That(reloaded.BossesDone, Is.EqualTo(bossesDoneBefore));
            });
        }

        [Test]
        public async Task UsersWithoutADiscordAccount_AreNotEligibleForRoles()
        {
            var channel = await GetChannel();
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            user.DiscordUserId = 0;

            var stats = await _dbContext.ChannelUserStats.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);
            stats.BitsDonatedThisMonth = 99999;
            await _dbContext.SaveChangesAsync();

            var eligible = await _dbContext.ChannelUserStats
                .Where(x => x.ChannelId == channel.Id && x.User.DiscordUserId != 0 && x.BitsDonatedThisMonth > 0)
                .Select(x => x.User.TwitchUsername)
                .ToListAsync();

            Assert.That(eligible, Does.Not.Contain(DatabaseSeedHelper.Channel1User1TwitchUsername));
        }
    }
}
