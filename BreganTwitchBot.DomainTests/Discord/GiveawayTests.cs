using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.Giveaway;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class GiveawayTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private DiscordGiveawayData _giveawayData;

        private const ulong StarterUserId = DatabaseSeedHelper.DiscordUserId1;
        private const ulong EntrantUserId = DatabaseSeedHelper.DiscordUserId2;

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

            // the giveaway channel has to match for a giveaway to start
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            config.DiscordGiveawayChannelId = GiveawayChannelId;
            await _dbContext.SaveChangesAsync();

            _giveawayData = new DiscordGiveawayData(_dbContext);
        }

        private const ulong GiveawayChannelId = 555666777;

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

        private async Task<string> StartGiveaway(int minimumHours = 0, string? requiredRank = null)
        {
            var (giveawayId, _) = await _giveawayData.StartGiveaway(DatabaseSeedHelper.DiscordGuildId, GiveawayChannelId, StarterUserId, minimumHours, requiredRank);
            return giveawayId!;
        }

        private async Task SetWatchtime(ulong discordUserId, int minutesInStream)
        {
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == discordUserId);
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var watchtime = await _dbContext.ChannelUserWatchtime.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            if (watchtime == null)
            {
                await _dbContext.ChannelUserWatchtime.AddAsync(new ChannelUserWatchtime
                {
                    ChannelUserId = user.Id,
                    ChannelId = channel.Id,
                    MinutesInStream = minutesInStream,
                    MinutesWatchedThisStream = 0,
                    MinutesWatchedThisWeek = 0,
                    MinutesWatchedThisMonth = 0,
                    MinutesWatchedThisYear = 0
                });
            }
            else
            {
                watchtime.MinutesInStream = minutesInStream;
            }

            await _dbContext.SaveChangesAsync();
        }

        [Test]
        public async Task StartGiveaway_InTheGiveawayChannel_Starts()
        {
            var (giveawayId, response) = await _giveawayData.StartGiveaway(DatabaseSeedHelper.DiscordGuildId, GiveawayChannelId, StarterUserId, 0, null);

            Assert.Multiple(() =>
            {
                Assert.That(giveawayId, Is.Not.Null);
                Assert.That(response, Does.Contain("A new giveaway has started"));
            });
        }

        [Test]
        public async Task StartGiveaway_InTheWrongChannel_DoesNotStart()
        {
            var (giveawayId, response) = await _giveawayData.StartGiveaway(DatabaseSeedHelper.DiscordGuildId, 999999, StarterUserId, 0, null);

            Assert.Multiple(() =>
            {
                Assert.That(giveawayId, Is.Null);
                Assert.That(response, Does.Contain("unstarted"));
            });
        }

        [Test]
        public async Task StartGiveaway_WithRequirements_StatesThemUpFront()
        {
            var (_, response) = await _giveawayData.StartGiveaway(DatabaseSeedHelper.DiscordGuildId, GiveawayChannelId, StarterUserId, 10, null);

            Assert.That(response, Does.Contain("10 hours"));
        }

        [Test]
        public async Task StartGiveaway_UnknownRank_DoesNotStart()
        {
            var (giveawayId, response) = await _giveawayData.StartGiveaway(DatabaseSeedHelper.DiscordGuildId, GiveawayChannelId, StarterUserId, 0, "notarank");

            Assert.Multiple(() =>
            {
                Assert.That(giveawayId, Is.Null);
                Assert.That(response, Does.Contain("isn't a rank called notarank"));
            });
        }

        [Test]
        public async Task EnterGiveaway_NoRequirements_Enters()
        {
            var giveawayId = await StartGiveaway();

            var (response, _) = await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            Assert.That(response, Does.Contain("You have entered the giveaway"));
        }

        [Test]
        public async Task EnterGiveaway_Twice_IsRejected()
        {
            var giveawayId = await StartGiveaway();

            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);
            var (response, _) = await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            Assert.That(response, Does.Contain("already entered"));
        }

        [Test]
        public async Task EnterGiveaway_BelowMinimumWatchtime_IsToldTheyDontQualify()
        {
            // the old bot silently entered these users with entries that could never win
            await SetWatchtime(EntrantUserId, 60);
            var giveawayId = await StartGiveaway(minimumHours: 10);

            var (response, _) = await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Does.Contain("You need"));
                Assert.That(response, Does.Contain("10 hours"));
                Assert.That(response, Does.Not.Contain("You have entered"));
            });
        }

        [Test]
        public async Task EnterGiveaway_BelowMinimumWatchtime_CreatesNoEntry()
        {
            await SetWatchtime(EntrantUserId, 60);
            var giveawayId = await StartGiveaway(minimumHours: 10);

            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            var giveaway = await _dbContext.DiscordGiveaways.FirstAsync(x => x.GiveawayId == giveawayId);
            var entryCount = await _dbContext.DiscordGiveawayEntries.CountAsync(x => x.DiscordGiveawayId == giveaway.Id);

            Assert.That(entryCount, Is.EqualTo(0));
        }

        [Test]
        public async Task EnterGiveaway_MeetingMinimumWatchtime_Enters()
        {
            await SetWatchtime(EntrantUserId, 1200);
            var giveawayId = await StartGiveaway(minimumHours: 10);

            var (response, _) = await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            Assert.That(response, Does.Contain("You have entered the giveaway"));
        }

        [Test]
        public async Task EnterGiveaway_MoreWatchtime_EarnsMoreEntries()
        {
            await SetWatchtime(EntrantUserId, 3600 * 5);
            var giveawayId = await StartGiveaway();

            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            var giveaway = await _dbContext.DiscordGiveaways.FirstAsync(x => x.GiveawayId == giveawayId);
            var entry = await _dbContext.DiscordGiveawayEntries.FirstAsync(x => x.DiscordGiveawayId == giveaway.Id && x.DiscordUserId == EntrantUserId);

            // 5 entries from watchtime at the default 3600 minutes per entry
            Assert.That(entry.Entries, Is.GreaterThanOrEqualTo(5));
        }

        [Test]
        public async Task EnterGiveaway_QualifyingWithNoWatchtime_StillGetsOneEntry()
        {
            var giveawayId = await StartGiveaway();

            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            var giveaway = await _dbContext.DiscordGiveaways.FirstAsync(x => x.GiveawayId == giveawayId);
            var entry = await _dbContext.DiscordGiveawayEntries.FirstAsync(x => x.DiscordGiveawayId == giveaway.Id);

            Assert.That(entry.Entries, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task EnterGiveaway_AfterItIsDrawn_IsRejected()
        {
            var giveawayId = await StartGiveaway();
            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);
            await _giveawayData.DrawWinner(DatabaseSeedHelper.DiscordGuildId, StarterUserId, giveawayId);

            var (response, _) = await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, StarterUserId, giveawayId);

            Assert.That(response, Does.Contain("finished"));
        }

        [Test]
        public async Task CheckEntries_AsEntrant_ShowsTheirEntries()
        {
            var giveawayId = await StartGiveaway();
            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            var (response, _) = await _giveawayData.CheckEntries(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            Assert.That(response, Does.Contain("You have entered the giveaway"));
        }

        [Test]
        public async Task CheckEntries_NotEntered_SaysSo()
        {
            var giveawayId = await StartGiveaway();

            var (response, _) = await _giveawayData.CheckEntries(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            Assert.That(response, Does.Contain("haven't entered"));
        }

        [Test]
        public async Task CheckEntries_AsStarter_ShowsTotals()
        {
            var giveawayId = await StartGiveaway();
            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            var (response, _) = await _giveawayData.CheckEntries(DatabaseSeedHelper.DiscordGuildId, StarterUserId, giveawayId);

            Assert.That(response, Does.Contain("people in the giveaway"));
        }

        [Test]
        public async Task DrawWinner_AsStarter_PicksSomeone()
        {
            var giveawayId = await StartGiveaway();
            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            var (response, ephemeral) = await _giveawayData.DrawWinner(DatabaseSeedHelper.DiscordGuildId, StarterUserId, giveawayId);

            Assert.Multiple(() =>
            {
                Assert.That(response, Does.Contain($"<@{EntrantUserId}>"));
                Assert.That(ephemeral, Is.False);
            });
        }

        [Test]
        public async Task DrawWinner_AsSomeoneElse_IsRejected()
        {
            var giveawayId = await StartGiveaway();
            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            var (response, _) = await _giveawayData.DrawWinner(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            Assert.That(response, Does.Contain("Only whoever started"));
        }

        [Test]
        public async Task DrawWinner_NobodyEntered_SaysSo()
        {
            var giveawayId = await StartGiveaway();

            var (response, _) = await _giveawayData.DrawWinner(DatabaseSeedHelper.DiscordGuildId, StarterUserId, giveawayId);

            Assert.That(response, Does.Contain("Nobody entered"));
        }

        [Test]
        public async Task DrawWinner_ClosesTheGiveaway()
        {
            var giveawayId = await StartGiveaway();
            await _giveawayData.EnterGiveaway(DatabaseSeedHelper.DiscordGuildId, EntrantUserId, giveawayId);

            await _giveawayData.DrawWinner(DatabaseSeedHelper.DiscordGuildId, StarterUserId, giveawayId);

            var giveaway = await _dbContext.DiscordGiveaways.FirstAsync(x => x.GiveawayId == giveawayId);

            Assert.Multiple(() =>
            {
                Assert.That(giveaway.Active, Is.False);
                Assert.That(giveaway.WinnerDiscordUserId, Is.EqualTo(EntrantUserId));
            });
        }
    }
}
