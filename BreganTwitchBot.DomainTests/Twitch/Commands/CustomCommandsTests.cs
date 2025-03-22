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
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.CustomCommands;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using System.Data;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class CustomCommandsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private Mock<ITwitchHelperService> _twitchHelperServiceMock;
        private Mock<ICommandHandler> _commandHandlerMock;

        private CustomCommandsDataService _customCommandsDataService;

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

            _twitchHelperServiceMock = new Mock<ITwitchHelperService>();
            _commandHandlerMock = new Mock<ICommandHandler>();

            _twitchHelperServiceMock.Setup(x => x.SendTwitchMessageToChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            _customCommandsDataService = new CustomCommandsDataService(_dbContext, _twitchHelperServiceMock.Object, _commandHandlerMock.Object);
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
        public async Task HandleCustomCommandAsync_CommandNotFound_LogsInformation()
        {
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!unknown",
                MessageId = "123",
                MessageParts = new string[] { "!unknown" }
            };

            await _customCommandsDataService.HandleCustomCommandAsync("!unknown", msgParams);
            _twitchHelperServiceMock.Verify(x => x.SendTwitchMessageToChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task HandleCustomCommandAsync_CommandOnCooldown_LogsInformation()
        {
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!oncooldown",
                MessageId = "123",
                MessageParts = new string[] { "!oncooldown" }
            };

            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            await _customCommandsDataService.HandleCustomCommandAsync("!oncooldown", msgParams);

            _twitchHelperServiceMock.Verify(x => x.SendTwitchMessageToChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task HandleCustomCommandAsync_CommandOnCooldownAndUserIsSuperMod_CommandSendsAndUpdates()
        {
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1SuperModUserTwitchUsername,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!oncooldown",
                MessageId = "123",
                MessageParts = new string[] { "!oncooldown" }
            };

            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            await _customCommandsDataService.HandleCustomCommandAsync("!oncooldown", msgParams);

            var commandData = await _dbContext.CustomCommands.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == "!oncooldown");

            _twitchHelperServiceMock.Verify(x => x.SendTwitchMessageToChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(commandData.TimesUsed, Is.EqualTo(1));
                Assert.That(commandData.LastUsed.Date, Is.EqualTo(DateTime.UtcNow.AddDays(1).Date));
            });
        }

        [Test]
        [TestCase("!readytouse")]
        [TestCase("!readytouse         ")]
        [TestCase("!READYTOUSE")]
        public async Task HandleCustomCommandAsync_CommandReadyToUse_CommandSendsAndUpdates(string commandName)
        {
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = commandName,
                MessageId = "123",
                MessageParts = new string[] { commandName }
            };

            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            await _customCommandsDataService.HandleCustomCommandAsync(commandName, msgParams);

            var commandData = await _dbContext.CustomCommands.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == "!readytouse");
            _twitchHelperServiceMock.Verify(x => x.SendTwitchMessageToChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(commandData.TimesUsed, Is.EqualTo(1));
                Assert.That(commandData.LastUsed, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
            });
        }

        [Test]
        public void AddNewCustomCommandAsync_CommandPartsLessThan3_ThrowsInvalidCommandException()
        {
            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = true,
                IsSub = false,
                IsVip = false,
                Message = "!addcmd",
                MessageId = "123",
                MessageParts = new string[] { "!addcmd" }
            };

            Assert.ThrowsAsync<InvalidCommandException>(async () => await _customCommandsDataService.AddNewCustomCommandAsync(msgParams));
        }

        [Test]
        public void AddNewCustomCommandAsync_UserNotModOrBroadcaster_ThrowsUnauthorizedAccessException()
        {
            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addcmd !newcommand commandText",
                MessageId = "123",
                MessageParts = new string[] { "!addcmd", "!newcommand", "commandText" }
            };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _customCommandsDataService.AddNewCustomCommandAsync(msgParams));
        }

        [Test]
        public void AddNewCustomCommandAsync_CommandIsSystemCommand_ThrowsInvalidCommandException()
        {
            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _commandHandlerMock.Setup(x => x.IsSystemCommand(It.IsAny<string>())).Returns(true);

            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = true,
                IsSub = false,
                IsVip = false,
                Message = "!addcmd !followage commandText",
                MessageId = "123",
                MessageParts = new string[] { "!addcmd", "!followage", "commandText" }
            };

            Assert.ThrowsAsync<DuplicateNameException>(async () => await _customCommandsDataService.AddNewCustomCommandAsync(msgParams));
        }

        [Test]
        public void AddNewCustomCommandAsync_CommandAlreadyExists_ThrowsDuplicateNameException()
        {
            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = true,
                IsSub = false,
                IsVip = false,
                Message = "!addcmd !readytouse commandText",
                MessageId = "123",
                MessageParts = new string[] { "!addcmd", "!readytouse", "commandText" }
            };

            Assert.ThrowsAsync<DuplicateNameException>(async () => await _customCommandsDataService.AddNewCustomCommandAsync(msgParams));
        }

        [Test]
        public async Task AddNewCustomCommandAsync_CommandAddedAsSuperMod_CommandAddedToDatabase()
        {
            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addcmd !newcommand commandText",
                MessageId = "123",
                MessageParts = new string[] { "!addcmd", "!newcommand", "commandText" }
            };

            await _customCommandsDataService.AddNewCustomCommandAsync(msgParams);
            var commandData = await _dbContext.CustomCommands.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == "!newcommand");

            Assert.Multiple(() =>
            {
                Assert.That(commandData.CommandText, Is.EqualTo("commandText"));
                Assert.That(commandData.TimesUsed, Is.EqualTo(0));
                Assert.That(commandData.LastUsed, Is.EqualTo(DateTime.MinValue));
            });
        }

        [Test]
        public async Task AddNewCustomCommandAsync_CommandAddedAsBroadcaster_CommandAddedToDatabase()
        {
            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                IsBroadcaster = true,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addcmd !newcommand commandText",
                MessageId = "123",
                MessageParts = new string[] { "!addcmd", "!newcommand", "commandText" }
            };
            
            await _customCommandsDataService.AddNewCustomCommandAsync(msgParams);

            var commandData = await _dbContext.CustomCommands.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == "!newcommand");
            
            Assert.Multiple(() =>
            {
                Assert.That(commandData.CommandText, Is.EqualTo("commandText"));
                Assert.That(commandData.TimesUsed, Is.EqualTo(0));
                Assert.That(commandData.LastUsed, Is.EqualTo(DateTime.MinValue));
            });
        }

        [Test]
        public async Task AddNewCustomCommandAsync_CommandAddedAsModerator_CommandAddedToDatabase()
        {
            _twitchHelperServiceMock.Setup(x => x.IsUserSuperModInChannel(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            var msgParams = new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                IsBroadcaster = false,
                IsMod = true,
                IsSub = false,
                IsVip = false,
                Message = "!addcmd !newcommand commandText",
                MessageId = "123",
                MessageParts = new string[] { "!addcmd", "!newcommand", "commandText" }
            };

            await _customCommandsDataService.AddNewCustomCommandAsync(msgParams);
            var commandData = await _dbContext.CustomCommands.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.CommandName == "!newcommand");

            Assert.Multiple(() =>
            {
                Assert.That(commandData.CommandText, Is.EqualTo("commandText"));
                Assert.That(commandData.TimesUsed, Is.EqualTo(0));
                Assert.That(commandData.LastUsed, Is.EqualTo(DateTime.MinValue));
            });
        }

    }
}
