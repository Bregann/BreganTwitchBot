using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using TwitchLib.Api;
using BreganTwitchBot.DomainTests.Helpers;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Hours;
using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    public class HoursCommandsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;
        private Mock<ITwitchApiConnection> _twitchApiConnection;
        private Mock<ITwitchHelperService> _twitchHelperService;
        private Mock<IDiscordLinkingData> _discordLinkingService;

        private HoursDataService _hoursDataService;

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
            _twitchHelperService = new Mock<ITwitchHelperService>();
            _discordLinkingService = new Mock<IDiscordLinkingData>();

            //Mock channel 1 broadcaster to be correct
            _twitchApiConnection.Setup(x => x.GetBotTwitchApiClientFromBroadcasterChannelId(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), 1, "", "", "", "", Domain.Enums.AccountType.Broadcaster, "", ""));

            // Mock channel 2 broadcaster to be null
            _twitchApiConnection.Setup(x => x.GetTwitchApiClientFromChannelName(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName))
                .Returns(value: null);

            _hoursDataService = new HoursDataService(_dbContext, _twitchApiInteractionService.Object, _twitchApiConnection.Object, _twitchHelperService.Object, _discordLinkingService.Object);
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
        public async Task UpdateWatchtimeForChannel_BroadcasterNotLive_WatchtimeNotUpdated()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = false;
            await _dbContext.SaveChangesAsync();

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var userWatchtime = await _dbContext.ChannelUserWatchtime.FirstAsync(x => x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.Multiple(() =>
            {
                Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(0));
                Assert.That(userWatchtime.MinutesWatchedThisWeek, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesWatchedThisMonth, Is.EqualTo(2));
                Assert.That(userWatchtime.MinutesWatchedThisYear, Is.EqualTo(3));
                Assert.That(userWatchtime.MinutesInStream, Is.EqualTo(4));
            });
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_ApiClientNull_DoesNotUpdateWatchtime()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);

            var userWatchtime = await _dbContext.ChannelUserWatchtime
                .FirstOrDefaultAsync(x => x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.ChannelId == channel.Id);

            if (userWatchtime != null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(0));
                    Assert.That(userWatchtime.MinutesWatchedThisWeek, Is.EqualTo(1));
                    Assert.That(userWatchtime.MinutesWatchedThisMonth, Is.EqualTo(2));
                    Assert.That(userWatchtime.MinutesWatchedThisYear, Is.EqualTo(3));
                    Assert.That(userWatchtime.MinutesInStream, Is.EqualTo(4));
                });
            }

            _twitchApiInteractionService.Verify(x => x.GetChattersAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_NewUser_AddsUserAndWatchtime()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            var newUser = new Chatters { UserId = "newuser123", UserName = "NewUser" };
            var chattersResult = new GetChattersResult { Chatters = new List<Chatters> { newUser } };

            _twitchApiInteractionService.Setup(x => x.GetChattersAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chattersResult);

            _twitchHelperService.Setup(x => x.AddOrUpdateUserToDatabase(channel.BroadcasterTwitchChannelId, newUser.UserId, channel.BroadcasterTwitchChannelName, newUser.UserName, true))
                .Returns(Task.CompletedTask);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _twitchHelperService.Verify(x => x.AddOrUpdateUserToDatabase(
                channel.BroadcasterTwitchChannelId,
                newUser.UserId,
                channel.BroadcasterTwitchChannelName,
                newUser.UserName,
                true), Times.Once);
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_ExistingUser_WatchtimeUpdates()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            var newUser = new Chatters { UserId = DatabaseSeedHelper.Channel1User1TwitchUserId, UserName = DatabaseSeedHelper.Channel1User1TwitchUsername };
            var chattersResult = new GetChattersResult { Chatters = new List<Chatters> { newUser } };

            _twitchApiInteractionService.Setup(x => x.GetChattersAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chattersResult);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var userWatchtime = await _dbContext.ChannelUserWatchtime.FirstAsync(x => x.ChannelUser.TwitchUsername == DatabaseSeedHelper.Channel1User1TwitchUsername);

            Assert.Multiple(() =>
            {
                Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesWatchedThisWeek, Is.EqualTo(2));
                Assert.That(userWatchtime.MinutesWatchedThisMonth, Is.EqualTo(3));
                Assert.That(userWatchtime.MinutesWatchedThisYear, Is.EqualTo(4));
                Assert.That(userWatchtime.MinutesInStream, Is.EqualTo(5));
            });
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_ExistingUserToBotButNewToUser_WatchtimeAdds()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            var newUser = new Chatters { UserId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId, UserName = DatabaseSeedHelper.Channel1SuperModUserTwitchUsername };
            var chattersResult = new GetChattersResult { Chatters = new List<Chatters> { newUser } };

            _twitchApiInteractionService.Setup(x => x.GetChattersAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chattersResult);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var userWatchtime = await _dbContext.ChannelUserWatchtime.FirstAsync(x => x.ChannelUser.TwitchUsername == DatabaseSeedHelper.Channel1SuperModUserTwitchUsername);

            Assert.Multiple(() =>
            {
                Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesWatchedThisWeek, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesWatchedThisMonth, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesWatchedThisYear, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesInStream, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_UserReachesRankup_RankUpAddsToDatabase()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            var newUser = new Chatters { UserId = DatabaseSeedHelper.Channel1User2TwitchUserId, UserName = DatabaseSeedHelper.Channel1User2TwitchUsername };
            var chattersResult = new GetChattersResult { Chatters = new List<Chatters> { newUser } };

            _twitchApiInteractionService.Setup(x => x.GetChattersAsync(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chattersResult);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var userRanks = await _dbContext.ChannelUserRankProgress.FirstAsync(x => x.ChannelUser.TwitchUsername == DatabaseSeedHelper.Channel1User2TwitchUsername);

            Assert.That(userRanks, Is.Not.Null);
        }

        [Test]
        [TestCase(HoursWatchTypes.Stream, "this stream!")]
        [TestCase(HoursWatchTypes.Week, "this week!")]
        [TestCase(HoursWatchTypes.Month, "this month!")]
        [TestCase(HoursWatchTypes.AllTime, "in the stream!")]
        public async Task GetHoursCommand_ValidUser_ReturnsCorrectMessage(HoursWatchTypes hoursType, string expectedMessageType)
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours" });
            await _dbContext.SaveChangesAsync();

            var response = await _hoursDataService.GetHoursCommand(msgParams, hoursType);

            Assert.That(response, Does.Contain(expectedMessageType));
            Assert.That(response, Does.Contain("about"));
        }

        [Test]
        public void GetHoursCommand_InvalidUser_ThrowsTwitchUserNotFoundException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours invalid", "123", new string[] { "!hours", "invalid" });

            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var ex = Assert.ThrowsAsync<TwitchUserNotFoundException>(() => _hoursDataService.GetHoursCommand(msgParams, HoursWatchTypes.AllTime));
            Assert.That(ex.Message, Is.EqualTo("User not found!"));
        }

        [Test]
        public void GetHoursCommand_UserWithNoWatchtime_ThrowsTwitchUserNotFoundException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours" }, chatterChannelId: "invalid", chatterChannelName: "invalid");

            var ex = Assert.ThrowsAsync<TwitchUserNotFoundException>(() => _hoursDataService.GetHoursCommand(msgParams, HoursWatchTypes.AllTime));
            Assert.That(ex.Message, Is.EqualTo("Oh dear this user doesn't have any watchtime in the channel!"));
        }

        [Test]
        public void GetHoursCommand_InvalidHoursType_ThrowsArgumentOutOfRangeException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours" });

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _hoursDataService.GetHoursCommand(msgParams, (HoursWatchTypes)999));
        }

        [Test]
        [TestCase(HoursWatchTypes.Stream, "this stream!")]
        [TestCase(HoursWatchTypes.Week, "this week!")]
        [TestCase(HoursWatchTypes.Month, "this month!")]
        [TestCase(HoursWatchTypes.AllTime, "in the stream!")]
        public async Task GetHoursCommand_ValidOtherUser_ReturnsCorrectMessage(HoursWatchTypes hoursType, string expectedMessageType)
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours", "cooluser2" });
            await _dbContext.SaveChangesAsync();

            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>()))
                .ReturnsAsync("789");

            var response = await _hoursDataService.GetHoursCommand(msgParams, hoursType);

            Assert.That(response, Does.Contain(expectedMessageType));
            Assert.That(response, Does.Contain("cooluser2 has"));
        }
    }
}
