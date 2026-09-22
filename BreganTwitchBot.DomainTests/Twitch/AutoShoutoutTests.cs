using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Events;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;
using TwitchLib.Api;

namespace BreganTwitchBot.DomainTests.Twitch
{
    [TestFixture]
    public class AutoShoutoutTests
    {
        private PostgreSqlContainer _postgresContainer;
        private ServiceProvider _serviceProvider;
        private AppDbContext _dbContext;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;
        private Mock<IConfigHelperService> _configHelperService;

        private TwitchEventHandlerService _eventHandlerService;

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
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseLazyLoadingProxies().UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped);

            _serviceProvider = services.BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();
            _configHelperService = new Mock<IConfigHelperService>();

            var twitchApiConnection = new Mock<ITwitchApiConnection>();
            twitchApiConnection.Setup(x => x.GetBotApiClient())
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), "", "", "", "", AccountType.Bot));

            _eventHandlerService = new TwitchEventHandlerService(
                new Mock<ITwitchHelperService>().Object,
                _twitchApiInteractionService.Object,
                twitchApiConnection.Object,
                _configHelperService.Object,
                new Mock<IDiscordHelperService>().Object,
                _serviceProvider,
                new Mock<IStreamStatsService>().Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
            _serviceProvider.Dispose();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _postgresContainer.DisposeAsync();
        }

        private async Task RaidWith(int viewers, int minimum)
        {
            _configHelperService.Setup(x => x.GetAutoShoutoutMinimumViewers(It.IsAny<string>())).Returns(minimum);

            await _eventHandlerService.HandleRaidEvent(new ChannelRaidParams
            {
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                RaidingChannelId = "raiderid",
                RaidingChannelName = "raiderchannel",
                Viewers = viewers
            });
        }

        private void VerifyShoutout(Times times)
        {
            _twitchApiInteractionService.Verify(x => x.ShoutoutChannel(
                It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), times);
        }

        [Test]
        public async Task RaidAboveTheMinimum_IsShoutedOut()
        {
            await RaidWith(viewers: 20, minimum: 5);

            VerifyShoutout(Times.Once());
        }

        [Test]
        public async Task RaidBelowTheMinimum_IsNotShoutedOut()
        {
            await RaidWith(viewers: 2, minimum: 5);

            VerifyShoutout(Times.Never());
        }

        [Test]
        public async Task RaidExactlyAtTheMinimum_IsShoutedOut()
        {
            // the configured number means what it says, rather than being one off
            await RaidWith(viewers: 5, minimum: 5);

            VerifyShoutout(Times.Once());
        }

        [Test]
        public async Task MinimumOfZero_ShoutsOutEveryRaid()
        {
            await RaidWith(viewers: 1, minimum: 0);

            VerifyShoutout(Times.Once());
        }

        [Test]
        public async Task AFailingShoutout_DoesNotThrow()
        {
            // twitch rate limits shoutouts, and that must not take down raid handling
            _twitchApiInteractionService
                .Setup(x => x.ShoutoutChannel(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("rate limited"));

            Assert.DoesNotThrowAsync(async () => await RaidWith(viewers: 20, minimum: 5));
        }
    }
}
