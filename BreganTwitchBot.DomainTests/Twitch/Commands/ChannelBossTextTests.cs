using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    /// <summary>
    /// The boss fight itself runs on timers, so these cover the data side: that a channel's
    /// own texts are stored and read per channel and per type.
    /// </summary>
    [TestFixture]
    public class ChannelBossTextTests
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

        private async Task<Channel> AddTexts()
        {
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _dbContext.ChannelBossTexts.AddRange(
                new ChannelBossText { ChannelId = channel.Id, TextType = BossTextType.BossSuffix, Text = "The Magnificent" },
                new ChannelBossText { ChannelId = channel.Id, TextType = BossTextType.BossSuffix, Text = "The Dreadful" },
                new ChannelBossText { ChannelId = channel.Id, TextType = BossTextType.EliminationReason, Text = "tripped over a cable" });

            await _dbContext.SaveChangesAsync();
            return channel;
        }

        [Test]
        public async Task TextsAreStoredPerType()
        {
            var channel = await AddTexts();

            var suffixes = await _dbContext.ChannelBossTexts
                .Where(x => x.ChannelId == channel.Id && x.TextType == BossTextType.BossSuffix)
                .Select(x => x.Text)
                .ToListAsync();

            var reasons = await _dbContext.ChannelBossTexts
                .Where(x => x.ChannelId == channel.Id && x.TextType == BossTextType.EliminationReason)
                .Select(x => x.Text)
                .ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(suffixes, Has.Count.EqualTo(2));
                Assert.That(reasons, Has.Count.EqualTo(1));
                Assert.That(suffixes, Does.Contain("The Magnificent"));
                Assert.That(reasons, Does.Contain("tripped over a cable"));
            });
        }

        [Test]
        public async Task TextsAreStoredPerChannel()
        {
            await AddTexts();

            var channel2 = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);
            var channel2Texts = await _dbContext.ChannelBossTexts.CountAsync(x => x.ChannelId == channel2.Id);

            Assert.That(channel2Texts, Is.EqualTo(0), "one channel's boss texts should not appear in another");
        }

        [Test]
        public async Task AChannelWithNoTexts_HasNoneStored()
        {
            // the service falls back to the built in lists in this case, so the feature still
            // works before anything is configured
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(await _dbContext.ChannelBossTexts.CountAsync(x => x.ChannelId == channel.Id), Is.EqualTo(0));
        }
    }
}
