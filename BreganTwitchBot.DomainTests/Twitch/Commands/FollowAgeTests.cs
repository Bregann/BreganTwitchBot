using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Commands.FollowAge;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;
using TwitchLib.Api;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class FollowAgeTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;
        private Mock<ITwitchApiConnection> _twitchApiConnection;

        private FollowAgeDataService _followAgeDataService;

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
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseLazyLoadingProxies()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            _dbContext = new AppDbContext(options);
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();
            _twitchApiConnection = new Mock<ITwitchApiConnection>();

            //Mock channel 1 broadcaster to be correct
            _twitchApiConnection.Setup(x => x.GetTwitchApiClientFromChannelName(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName))
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), 1, "", "", "", "", Domain.Enums.AccountType.Broadcaster, "", ""));

            // Mock channel 2 broadcaster to be null
            _twitchApiConnection.Setup(x => x.GetTwitchApiClientFromChannelName(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName))
                .Returns(value: null);

            _followAgeDataService = new FollowAgeDataService(_twitchApiConnection.Object, _twitchApiInteractionService.Object);
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
        public void HandleFollowCommandAsync_ProvideIncorrectBroadcasterChannelName_CorrectExceptionThrown()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!followage", "123", new[] { "!followage" }, broadcasterChannelName: "invalid");

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _followAgeDataService.HandleFollowCommandAsync(msgParams, FollowCommandTypeEnum.FollowAge);
            });

            Assert.That(ex.Message, Is.EqualTo("Could not get the twitch api client from the broadcaster username"));
        }

        [Test]
        public async Task HandleFollowCommandAsync_ProvideInvalidOtherTwitchUsername_CorrectResultReturned()
        {
            _twitchApiInteractionService.Setup(x => x.GetUsersAsync(It.IsAny<TwitchAPI>(), "CoolUser1"))
                .ReturnsAsync(value: null);

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!followage CoolUser1", "123", new[] { "!followage", "CoolUser1" });
            var result = await _followAgeDataService.HandleFollowCommandAsync(msgParams, FollowCommandTypeEnum.FollowAge);

            Assert.That(result, Is.EqualTo("That username does not exist!"));
        }

        [Test]
        [TestCase(FollowCommandTypeEnum.FollowAge)]
        [TestCase(FollowCommandTypeEnum.FollowSince)]
        [TestCase(FollowCommandTypeEnum.FollowMinutes)]
        public async Task HandleFollowCommandAsync_MockCommandUserNotFollowingChannel_CorrectResultReturned(FollowCommandTypeEnum followTypeEnum)
        {
            _twitchApiInteractionService.Setup(x => x.GetUsersAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1User1TwitchUsername))
                .ReturnsAsync(new GetUsersAsyncResponse
                {
                    Users =
                    [
                        new User
                        {
                            DisplayName =   DatabaseSeedHelper.Channel1User1TwitchUsername,
                            Id = DatabaseSeedHelper.Channel1User1TwitchUserId,
                            Login = ""
                        }
                    ]
                });

            _twitchApiInteractionService.Setup(x => x.GetChannelFollowersAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId))
                .ReturnsAsync(new GetChannelFollowersAsyncResponse
                {
                    Followers = [],
                    Total = 0
                });

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!followage", "123", new[] { "!followage" });

            var result = await _followAgeDataService.HandleFollowCommandAsync(msgParams, followTypeEnum);

            Assert.That(result, Is.EqualTo($"It appears {DatabaseSeedHelper.Channel1User1TwitchUsername} doesn't follow {DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName} :("));
        }

        [Test]
        [TestCase(FollowCommandTypeEnum.FollowAge)]
        [TestCase(FollowCommandTypeEnum.FollowSince)]
        [TestCase(FollowCommandTypeEnum.FollowMinutes)]
        public async Task HandleFollowAgeCommandAsync_MockCommandUSerIsFollowingChannel_CorrectResultReturned(FollowCommandTypeEnum followCommandTypeEnum)
        {
            _twitchApiInteractionService.Setup(x => x.GetUsersAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1User1TwitchUsername))
                .ReturnsAsync(new GetUsersAsyncResponse
                {
                    Users =
                    [
                        new User
                        {
                            DisplayName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                            Id = DatabaseSeedHelper.Channel1User1TwitchUserId,
                            Login = ""
                        }
                    ]
                });

            _twitchApiInteractionService.Setup(x => x.GetChannelFollowersAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId))
                .ReturnsAsync(new GetChannelFollowersAsyncResponse
                {
                    Followers = [new ChannelFollower
                    {
                        FollowedAt = DateTime.UtcNow.AddDays(-1).ToString(),
                        UserId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                        UserLogin = DatabaseSeedHelper.Channel1User1TwitchUsername,
                        UserName = DatabaseSeedHelper.Channel1User1TwitchUsername
                    }],
                    Total = 1
                });

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!followage", "123", new[] { "!followage" });
            var result = await _followAgeDataService.HandleFollowCommandAsync(msgParams, followCommandTypeEnum);

            Assert.That(result, Does.Contain($"{DatabaseSeedHelper.Channel1User1TwitchUsername} followed {DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName}"));

            switch (followCommandTypeEnum)
            {
                case FollowCommandTypeEnum.FollowAge:
                    Assert.That(result, Does.Contain($"1 day"));
                    break;
                case FollowCommandTypeEnum.FollowSince:
                    Assert.That(result, Does.Contain(DateTime.UtcNow.AddDays(-1).ToString("MMMM dd, yyyy 'at' HH:mm")));
                    break;
                case FollowCommandTypeEnum.FollowMinutes:
                    Assert.That(result, Does.Contain($"1440 minutes"));
                    break;
            }
        }

        [Test]
        [TestCase(FollowCommandTypeEnum.FollowAge)]
        [TestCase(FollowCommandTypeEnum.FollowSince)]
        [TestCase(FollowCommandTypeEnum.FollowMinutes)]
        public async Task HandleFollowCommandAsync_MockOtherUserNotFollowingChannel_CorrectResultReturned(FollowCommandTypeEnum followCommandTypeEnum)
        {
            _twitchApiInteractionService.Setup(x => x.GetUsersAsync(It.IsAny<TwitchAPI>(), "CoolUser1"))
                .ReturnsAsync(new GetUsersAsyncResponse
                {
                    Users =
                    [
                        new User
                        {
                            DisplayName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                            Id = DatabaseSeedHelper.Channel1User1TwitchUserId,
                            Login = ""
                        }
                    ]
                });

            _twitchApiInteractionService.Setup(x => x.GetChannelFollowersAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId))
                .ReturnsAsync(new GetChannelFollowersAsyncResponse
                {
                    Followers = [],
                    Total = 0
                });

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!followage CoolUser1", "123", new[] { "!followage", "CoolUser1" });
            var result = await _followAgeDataService.HandleFollowCommandAsync(msgParams, followCommandTypeEnum);

            Assert.That(result, Is.EqualTo($"It appears CoolUser1 doesn't follow {DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName} :("));
        }

        [Test]
        [TestCase(FollowCommandTypeEnum.FollowAge)]
        [TestCase(FollowCommandTypeEnum.FollowSince)]
        [TestCase(FollowCommandTypeEnum.FollowMinutes)]
        public async Task HandleFollowCommandAsync_MockOtherUserIsFollowingChannel_CorrectResultReturned(FollowCommandTypeEnum followCommandTypeEnum)
        {
            _twitchApiInteractionService.Setup(x => x.GetUsersAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1User1TwitchUsername))
                .ReturnsAsync(new GetUsersAsyncResponse
                {
                    Users =
                    [
                        new User
                        {
                            DisplayName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                            Id = DatabaseSeedHelper.Channel1User1TwitchUserId,
                            Login = ""
                        }
                    ]
                });

            _twitchApiInteractionService.Setup(x => x.GetUsersAsync(It.IsAny<TwitchAPI>(), "CoolUser1"))
                .ReturnsAsync(new GetUsersAsyncResponse
                {
                    Users =
                    [
                        new User
                                    {
                                        DisplayName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                                        Id = DatabaseSeedHelper.Channel1User1TwitchUserId,
                                        Login = ""
                                    }
                    ]
                });

            _twitchApiInteractionService.Setup(x => x.GetChannelFollowersAsync(It.IsAny<TwitchAPI>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1User1TwitchUserId))
                .ReturnsAsync(new GetChannelFollowersAsyncResponse
                {
                    Followers = [new ChannelFollower
                    {
                        FollowedAt = DateTime.UtcNow.AddDays(-1).ToString(),
                        UserId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                        UserLogin = DatabaseSeedHelper.Channel1User1TwitchUsername,
                        UserName = DatabaseSeedHelper.Channel1User1TwitchUsername
                    }],
                    Total = 1
                });

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!followage CoolUser1", "123", new[] { "!followage", "CoolUser1" });

            var result = await _followAgeDataService.HandleFollowCommandAsync(msgParams, followCommandTypeEnum);
            Assert.That(result, Does.Contain($"CoolUser1 followed {DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName}"));

            switch (followCommandTypeEnum)
            {
                case FollowCommandTypeEnum.FollowAge:
                    Assert.That(result, Does.Contain($"1 day"));
                    break;
                case FollowCommandTypeEnum.FollowSince:
                    Assert.That(result, Does.Contain(DateTime.UtcNow.AddDays(-1).ToString("MMMM dd, yyyy 'at' HH:mm")));
                    break;
                case FollowCommandTypeEnum.FollowMinutes:
                    Assert.That(result, Does.Contain($"1440 minutes"));
                    break;
            }
        }
    }
}
