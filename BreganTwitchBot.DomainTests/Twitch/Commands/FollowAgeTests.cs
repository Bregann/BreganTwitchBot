using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Api;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
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
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _followAgeDataService.HandleFollowCommandAsync(new ChannelChatMessageReceivedParams
                {
                    BroadcasterChannelName = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName,
                    BroadcasterChannelId = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId,
                    ChatterChannelName = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName,
                    ChatterChannelId = DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId,
                    MessageParts = new[] { "!followage" },
                    Message = "!followage",
                    IsBroadcaster = false,
                    IsMod = false,
                    IsSub = false,
                    IsVip = false,
                    MessageId = "123",
                }, FollowCommandTypeEnum.FollowAge);
            });

            Assert.That(ex.Message, Is.EqualTo("Could not get the twitch api client from the broadcaster username"));
        }

        [Test]
        public async Task HandleFollowCommandAsync_ProvideInvalidOtherTwitchUsername_CorrectResultReturned()
        {
            _twitchApiInteractionService.Setup(x => x.GetUsersAsync(It.IsAny<TwitchAPI>(), "CoolUser1"))
                .ReturnsAsync(value: null);

            var result = await _followAgeDataService.HandleFollowCommandAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                MessageParts = new[] { "!followage", "CoolUser1" },
                Message = "!followage CoolUser1",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                MessageId = "123",
            }, FollowCommandTypeEnum.FollowAge);

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

            var result = await _followAgeDataService.HandleFollowCommandAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                MessageParts = new[] { "!followage" },
                Message = "!followage",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                MessageId = "123",
            }, followTypeEnum);

            Assert.That(result, Is.EqualTo($"It appears {DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName} doesn't follow {DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName} :("));
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

            var result = await _followAgeDataService.HandleFollowCommandAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                MessageParts = new[] { "!followage" },
                Message = "!followage",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                MessageId = "123",
            }, followCommandTypeEnum);

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

            var result = await _followAgeDataService.HandleFollowCommandAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                MessageParts = new[] { "!followage", "CoolUser1" },
                Message = "!followage CoolUser1",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                MessageId = "123",
            }, followCommandTypeEnum);

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

            var result = await _followAgeDataService.HandleFollowCommandAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                MessageParts = new[] { "!followage", "CoolUser1" },
                Message = "!followage CoolUser1",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                MessageId = "123",
            }, followCommandTypeEnum);
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
