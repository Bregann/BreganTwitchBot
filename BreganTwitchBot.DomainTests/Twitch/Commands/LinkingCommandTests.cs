using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Linking;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class LinkingCommandTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private LinkingDataService _linkingDataService;

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

            _linkingDataService = new LinkingDataService(_dbContext);
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
        public async Task HandleLinkCommand_WhenDiscordIsNotEnabled_ReturnsNull()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link", "123", ["!link"]);

            var result = await _linkingDataService.HandleLinkCommand(msgParams);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task HandleLinkCommand_WhenNoLinkRequestExists_ReturnsErrorMessage()
        {
            _dbContext.Channels.First().DiscordEnabled = true;
            await _dbContext.SaveChangesAsync();

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link", "123", ["!link"]);
            var result = await _linkingDataService.HandleLinkCommand(msgParams);
            Assert.That(result, Is.EqualTo("You have not requested to link your account yet. Please use /link in the discord server to link your account."));
        }

        [Test]
        public async Task HandleLinkCommand_WhenLinkRequestExists_LinksAccounts()
        {
            _dbContext.Channels.First().DiscordEnabled = true;
            await _dbContext.SaveChangesAsync();

            var linkRequest = new DiscordLinkRequests
            {
                ChannelId = 1,
                TwitchUserId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                DiscordUserId = 123
            };

            _dbContext.DiscordLinkRequests.Add(linkRequest);
            await _dbContext.SaveChangesAsync();

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!link", "123", ["!link"]);

            var result = await _linkingDataService.HandleLinkCommand(msgParams);
            Assert.That(result, Is.EqualTo("Your Twitch and Discord have been linked!"));

            var user = _dbContext.ChannelUsers.First(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            _dbContext.Entry(user).Reload();

            Assert.That(user.DiscordUserId, Is.EqualTo(123));
        }
    }
}
