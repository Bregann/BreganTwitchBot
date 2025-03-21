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
    }
}
