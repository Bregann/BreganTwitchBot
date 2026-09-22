using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Commands.Marbles;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class MarblesTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<ITwitchHelperService> _twitchHelperService;

        private MarblesDataService _marblesDataService;

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

            // by default the caller is a mod and the looked up user exists
            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(DatabaseSeedHelper.Channel1User1TwitchUsername))
                .ReturnsAsync(DatabaseSeedHelper.Channel1User1TwitchUserId);

            _marblesDataService = new MarblesDataService(_dbContext, _twitchHelperService.Object);
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

        private static ChannelChatMessageReceivedParams CreateMsgParams(string message, bool isMod = true, bool isBroadcaster = false)
        {
            return new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1SuperModUserTwitchUsername,
                Message = message,
                MessageParts = message.Split(' '),
                MessageId = "messageid",
                IsMod = isMod,
                IsSub = false,
                IsVip = false,
                IsBroadcaster = isBroadcaster
            };
        }

        [Test]
        public async Task AddMarblesWin_ValidUser_IncrementsWins()
        {
            var msgParams = CreateMsgParams($"!addmarbleswin {DatabaseSeedHelper.Channel1User1TwitchUsername}");

            var response = await _marblesDataService.AddMarblesWin(msgParams);

            var userStats = await _dbContext.ChannelUserStats
                .FirstAsync(x => x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.Multiple(() =>
            {
                Assert.That(userStats.MarblesWins, Is.EqualTo(1));
                Assert.That(response, Does.Contain("marbol win has been added"));
            });
        }

        [Test]
        public async Task AddMarblesWin_CalledTwice_IncrementsTwice()
        {
            var msgParams = CreateMsgParams($"!addmarbleswin {DatabaseSeedHelper.Channel1User1TwitchUsername}");

            await _marblesDataService.AddMarblesWin(msgParams);
            await _marblesDataService.AddMarblesWin(msgParams);

            var userStats = await _dbContext.ChannelUserStats
                .FirstAsync(x => x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(userStats.MarblesWins, Is.EqualTo(2));
        }

        [Test]
        public void AddMarblesWin_NoUsernameSupplied_ThrowsInvalidCommand()
        {
            var msgParams = CreateMsgParams("!addmarbleswin");

            Assert.ThrowsAsync<InvalidCommandException>(async () => await _marblesDataService.AddMarblesWin(msgParams));
        }

        [Test]
        public void AddMarblesWin_UnknownUsername_ThrowsUserNotFound()
        {
            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername("someonewhodoesnotexist"))
                .ReturnsAsync((string?)null);

            var msgParams = CreateMsgParams("!addmarbleswin someonewhodoesnotexist");

            Assert.ThrowsAsync<TwitchUserNotFoundException>(async () => await _marblesDataService.AddMarblesWin(msgParams));
        }

        [Test]
        public void AddMarblesWin_NotAMod_ThrowsUnauthorised()
        {
            _twitchHelperService
                .Setup(x => x.EnsureUserHasModeratorPermissions(It.IsAny<ChannelChatMessageReceivedParams>()))
                .ThrowsAsync(new UnauthorizedAccessException("You don't have permission to do that"));

            var msgParams = CreateMsgParams($"!addmarbleswin {DatabaseSeedHelper.Channel1User1TwitchUsername}", isMod: false);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _marblesDataService.AddMarblesWin(msgParams));
        }

        [Test]
        public async Task AddMarblesWin_UserNotInChannel_DoesNotAffectOtherChannel()
        {
            // channel 2 has no stats row seeded for this user
            var msgParams = CreateMsgParams($"!addmarbleswin {DatabaseSeedHelper.Channel1User1TwitchUsername}");
            msgParams.BroadcasterChannelId = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId;
            msgParams.BroadcasterChannelName = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName;

            Assert.ThrowsAsync<TwitchUserNotFoundException>(async () => await _marblesDataService.AddMarblesWin(msgParams));

            // the channel 1 total must be untouched
            var userStats = await _dbContext.ChannelUserStats
                .FirstAsync(x => x.User.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(userStats.MarblesWins, Is.EqualTo(0));
        }

        [Test]
        public async Task GetMarblesWins_NoUsernameSupplied_ReturnsCallersWins()
        {
            var msgParams = CreateMsgParams("!marbles");
            msgParams.ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId;
            msgParams.ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername;

            var response = await _marblesDataService.GetMarblesWins(msgParams);

            Assert.That(response, Does.Contain("has won 0 games of marbles"));
        }

        [Test]
        public async Task GetMarblesWins_UsernameSupplied_ReturnsThatUsersWins()
        {
            var addParams = CreateMsgParams($"!addmarbleswin {DatabaseSeedHelper.Channel1User1TwitchUsername}");
            await _marblesDataService.AddMarblesWin(addParams);

            var msgParams = CreateMsgParams($"!marbles {DatabaseSeedHelper.Channel1User1TwitchUsername}");
            var response = await _marblesDataService.GetMarblesWins(msgParams);

            Assert.That(response, Does.Contain($"{DatabaseSeedHelper.Channel1User1TwitchUsername} has won 1 games of marbles"));
        }

        [Test]
        public void GetMarblesWins_UnknownUsername_ThrowsUserNotFound()
        {
            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername("someonewhodoesnotexist"))
                .ReturnsAsync((string?)null);

            var msgParams = CreateMsgParams("!marbles someonewhodoesnotexist");

            Assert.ThrowsAsync<TwitchUserNotFoundException>(async () => await _marblesDataService.GetMarblesWins(msgParams));
        }
    }
}
