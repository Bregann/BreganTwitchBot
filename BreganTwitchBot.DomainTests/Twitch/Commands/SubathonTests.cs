using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Commands.Subathon;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class SubathonTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<ITwitchHelperService> _twitchHelperService;

        private SubathonDataService _subathonDataService;

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

            _twitchHelperService = new Mock<ITwitchHelperService>();
            _subathonDataService = new SubathonDataService(_dbContext, _twitchHelperService.Object);
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

        private static ChannelChatMessageReceivedParams CreateMsgParams(string message, bool isMod = true)
        {
            return new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                Message = message,
                MessageParts = message.Split(' '),
                MessageId = "messageid",
                IsMod = isMod,
                IsSub = false,
                IsVip = false,
                IsBroadcaster = false
            };
        }

        private async Task StartSubathon(TimeSpan startingTime)
        {
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            config.SubathonActive = true;
            config.SubathonTime = startingTime;
            config.SubathonStartTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        private async Task<TimeSpan> GetSubathonTime()
        {
            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            return config.SubathonTime;
        }

        // Bits - the rate tapers as the accumulated total grows

        [Test]
        public async Task AddBitsTime_UnderTwelveHours_UsesTopRate()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, 100);

            // 100 bits * 900ms = 90 seconds
            var time = await GetSubathonTime();
            Assert.That(time - TimeSpan.FromHours(1), Is.EqualTo(TimeSpan.FromSeconds(90)));
        }

        [Test]
        public async Task AddBitsTime_OverTwentyFourHours_UsesBottomRate()
        {
            await StartSubathon(TimeSpan.FromHours(30));

            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, 100);

            // 100 bits * 150ms = 15 seconds
            var time = await GetSubathonTime();
            Assert.That(time - TimeSpan.FromHours(30), Is.EqualTo(TimeSpan.FromSeconds(15)));
        }

        [Test]
        public async Task AddBitsTime_AtTwelveHourBand_UsesThatBandRate()
        {
            await StartSubathon(TimeSpan.FromHours(12));

            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, 100);

            // 100 bits * 750ms = 75 seconds
            var time = await GetSubathonTime();
            Assert.That(time - TimeSpan.FromHours(12), Is.EqualTo(TimeSpan.FromSeconds(75)));
        }

        [Test]
        public async Task AddBitsTime_CheerCrossingABand_DoesNotAllUseTheOldRate()
        {
            // just under the 12h band with a cheer big enough to push through it
            await StartSubathon(TimeSpan.FromHours(12) - TimeSpan.FromSeconds(9));

            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, 100);

            var added = await GetSubathonTime() - (TimeSpan.FromHours(12) - TimeSpan.FromSeconds(9));

            // all at 900ms would be 90s, all at 750ms would be 75s - crossing the band lands between
            Assert.Multiple(() =>
            {
                Assert.That(added, Is.LessThan(TimeSpan.FromSeconds(90)));
                Assert.That(added, Is.GreaterThan(TimeSpan.FromSeconds(75)));
            });
        }

        [Test]
        public async Task AddBitsTime_LargeCheer_CompletesWithoutLooping()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            // the old bot looped once per bit, so this was 500k iterations
            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, 500000);

            Assert.That(await GetSubathonTime(), Is.GreaterThan(TimeSpan.FromHours(1)));
        }

        [Test]
        public async Task AddBitsTime_SubathonNotActive_AddsNothing()
        {
            // the seed data leaves channel 1 with an hour banked but the subathon switched off,
            // so a cheer must leave that total untouched
            var timeBefore = await GetSubathonTime();

            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, 100);

            Assert.That(await GetSubathonTime(), Is.EqualTo(timeBefore));
        }

        // Subs

        [Test]
        public async Task AddSubTime_Tier1UnderTwelveHours_AddsSixMinutes()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            await _subathonDataService.AddSubTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, SubTierEnum.Tier1);

            Assert.That(await GetSubathonTime() - TimeSpan.FromHours(1), Is.EqualTo(TimeSpan.FromMinutes(6)));
        }

        [Test]
        public async Task AddSubTime_Tier3UnderTwelveHours_AddsThirtyMinutes()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            await _subathonDataService.AddSubTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, SubTierEnum.Tier3);

            Assert.That(await GetSubathonTime() - TimeSpan.FromHours(1), Is.EqualTo(TimeSpan.FromMinutes(30)));
        }

        [Test]
        public async Task AddSubTime_Tier1OverTwentyFourHours_AddsOneMinute()
        {
            await StartSubathon(TimeSpan.FromHours(30));

            await _subathonDataService.AddSubTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, SubTierEnum.Tier1);

            Assert.That(await GetSubathonTime() - TimeSpan.FromHours(30), Is.EqualTo(TimeSpan.FromMinutes(1)));
        }

        [Test]
        public async Task AddSubTime_MultipleGiftedSubs_AddsForEach()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            await _subathonDataService.AddSubTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, SubTierEnum.Tier1, 5);

            Assert.That(await GetSubathonTime() - TimeSpan.FromHours(1), Is.EqualTo(TimeSpan.FromMinutes(30)));
        }

        // Contributions

        [Test]
        public async Task Contributions_AreTrackedPerUser()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, 100);
            await _subathonDataService.AddSubTimeAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId, SubTierEnum.Tier1, 2);

            var contribution = await _dbContext.Subathons
                .FirstAsync(x => x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.Multiple(() =>
            {
                Assert.That(contribution.BitsDonated, Is.EqualTo(100));
                Assert.That(contribution.SubsGifted, Is.EqualTo(2));
                Assert.That(contribution.TimeAdded, Is.GreaterThan(TimeSpan.Zero));
            });
        }

        // Start / stop / manual

        [Test]
        public async Task StartSubathon_SetsActiveAndStartTime()
        {
            var response = await _subathonDataService.StartSubathonAsync(CreateMsgParams("!startsubathon"));

            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.Multiple(() =>
            {
                Assert.That(config.SubathonActive, Is.True);
                Assert.That(config.SubathonStartTime, Is.Not.Null);
                Assert.That(response, Does.Contain("subathon has started"));
            });
        }

        [Test]
        public async Task StartSubathon_WithStartingHours_SetsThatTime()
        {
            await _subathonDataService.StartSubathonAsync(CreateMsgParams("!startsubathon 5"));

            Assert.That(await GetSubathonTime(), Is.EqualTo(TimeSpan.FromHours(5)));
        }

        [Test]
        public async Task StartSubathon_WhenAlreadyRunning_DoesNotRestart()
        {
            await StartSubathon(TimeSpan.FromHours(3));

            var response = await _subathonDataService.StartSubathonAsync(CreateMsgParams("!startsubathon"));

            Assert.Multiple(async () =>
            {
                Assert.That(response, Does.Contain("already a subathon running"));
                Assert.That(await GetSubathonTime(), Is.EqualTo(TimeSpan.FromHours(3)));
            });
        }

        [Test]
        public void StartSubathon_AsNonMod_ThrowsUnauthorised()
        {
            _twitchHelperService
                .Setup(x => x.EnsureUserHasModeratorPermissions(false, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("You don't have permission to do that"));

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _subathonDataService.StartSubathonAsync(CreateMsgParams("!startsubathon", isMod: false)));
        }

        [Test]
        public async Task StopSubathon_SetsInactive()
        {
            await StartSubathon(TimeSpan.FromHours(2));

            var response = await _subathonDataService.StopSubathonAsync(CreateMsgParams("!stopsubathon"));

            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.Multiple(() =>
            {
                Assert.That(config.SubathonActive, Is.False);
                Assert.That(response, Does.Contain("subathon has ended"));
            });
        }

        [Test]
        public async Task AddTimeManually_AddsTheSeconds()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            await _subathonDataService.AddTimeManuallyAsync(CreateMsgParams("!addsubathontime 300"));

            Assert.That(await GetSubathonTime() - TimeSpan.FromHours(1), Is.EqualTo(TimeSpan.FromMinutes(5)));
        }

        [Test]
        public void AddTimeManually_NoSeconds_ThrowsInvalidCommand()
        {
            Assert.ThrowsAsync<InvalidCommandException>(async () =>
                await _subathonDataService.AddTimeManuallyAsync(CreateMsgParams("!addsubathontime")));
        }

        [Test]
        public void AddTimeManually_NotANumber_ThrowsInvalidCommand()
        {
            Assert.ThrowsAsync<InvalidCommandException>(async () =>
                await _subathonDataService.AddTimeManuallyAsync(CreateMsgParams("!addsubathontime abc")));
        }

        [Test]
        public async Task GetStatus_NoSubathonRunning_SaysSo()
        {
            var response = await _subathonDataService.GetSubathonStatusAsync(CreateMsgParams("!subathon"));

            Assert.That(response, Does.Contain("isn't a subathon running"));
        }

        [Test]
        public async Task GetStatus_Running_ReportsTotalAndTimeLeft()
        {
            await StartSubathon(TimeSpan.FromHours(2));

            var response = await _subathonDataService.GetSubathonStatusAsync(CreateMsgParams("!subathon"));

            Assert.Multiple(() =>
            {
                Assert.That(response, Does.Contain("extended to a total of"));
                Assert.That(response, Does.Contain("will end in"));
            });
        }

        [Test]
        public async Task SubathonOnAnotherChannel_IsNotAffected()
        {
            await StartSubathon(TimeSpan.FromHours(1));

            // channel 2 has no subathon running, so this must add nothing there
            await _subathonDataService.AddBitsTimeAsync(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel2User1TwitchUserId, 100);

            var channel2Config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);

            // channel 2 keeps its seeded hour untouched, and channel 1's running subathon is
            // unaffected by the cheer aimed at the other channel
            Assert.Multiple(async () =>
            {
                Assert.That(channel2Config.SubathonTime, Is.EqualTo(TimeSpan.FromHours(1)));
                Assert.That(await GetSubathonTime(), Is.EqualTo(TimeSpan.FromHours(1)));
            });
        }
    }
}
