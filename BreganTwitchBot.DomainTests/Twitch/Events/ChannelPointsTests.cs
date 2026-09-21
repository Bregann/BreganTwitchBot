using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Events;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Events
{
    [TestFixture]
    public class ChannelPointsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private ServiceProvider _serviceProvider;
        private AppDbContext _dbContext;
        private Mock<ITwitchHelperService> _twitchHelperService;

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
                options.UseLazyLoadingProxies()
                       .UseNpgsql(_postgresContainer.GetConnectionString()),
                ServiceLifetime.Scoped
            );

            _serviceProvider = services.BuildServiceProvider();

            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _twitchHelperService = new Mock<ITwitchHelperService>();

            _eventHandlerService = new TwitchEventHandlerService(
                _twitchHelperService.Object,
                new Mock<ITwitchApiInteractionService>().Object,
                new Mock<ITwitchApiConnection>().Object,
                new Mock<IConfigHelperService>().Object,
                new Mock<IDiscordHelperService>().Object,
                _serviceProvider
            );
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

        private static ChannelPointsRedeemedParams CreateRedemption(string rewardTitle, string status = "UNFULFILLED", string? broadcasterId = null)
        {
            return new ChannelPointsRedeemedParams
            {
                BroadcasterChannelId = broadcasterId ?? DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
                ChatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
                RewardId = "reward-id",
                RewardTitle = rewardTitle,
                RewardCost = 500,
                RedemptionStatus = status,
                UserInput = null
            };
        }

        [Test]
        public async Task ConfiguredReward_SendsTheConfiguredMessage()
        {
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(CreateRedemption(DatabaseSeedHelper.SeededChannel1RewardTitle));

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                $"{DatabaseSeedHelper.Channel1User1TwitchUsername} has redeemed Goose! Goose Goose Goose",
                null), Times.Once);
        }

        [Test]
        public async Task ConfiguredReward_IncrementsTimesRedeemed()
        {
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(CreateRedemption(DatabaseSeedHelper.SeededChannel1RewardTitle));

            // the handler writes in its own scope, so read back through a fresh context rather
            // than the one holding the seeded entity
            using var verifyScope = _serviceProvider.CreateScope();
            var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var reward = await verifyContext.ChannelPointRewards
                .FirstAsync(x => x.RewardTitle == DatabaseSeedHelper.SeededChannel1RewardTitle && x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(reward.TimesRedeemed, Is.EqualTo(1));
        }

        [Test]
        public async Task RewardTitle_IsMatchedCaseInsensitively()
        {
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(CreateRedemption("GoOsE"));

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task UnknownReward_SendsNothing()
        {
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(CreateRedemption("a reward nobody configured"));

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task DisabledReward_SendsNothing()
        {
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(CreateRedemption(DatabaseSeedHelper.SeededChannel1DisabledRewardTitle));

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task DisabledReward_DoesNotIncrementTimesRedeemed()
        {
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(CreateRedemption(DatabaseSeedHelper.SeededChannel1DisabledRewardTitle));

            using var verifyScope = _serviceProvider.CreateScope();
            var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var reward = await verifyContext.ChannelPointRewards
                .FirstAsync(x => x.RewardTitle == DatabaseSeedHelper.SeededChannel1DisabledRewardTitle);

            Assert.That(reward.TimesRedeemed, Is.EqualTo(0));
        }

        [Test]
        public async Task ActionTakenRedemption_IsIgnored()
        {
            // a mod or the broadcaster already handled this one in the redemption queue
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(CreateRedemption(DatabaseSeedHelper.SeededChannel1RewardTitle, status: "ACTION_TAKEN"));

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task RewardConfiguredOnAnotherChannel_DoesNotFire()
        {
            // channel 2 has no rewards configured, so goose must not fire there
            await _eventHandlerService.HandleChannelPointsRedeemedEvent(
                CreateRedemption(DatabaseSeedHelper.SeededChannel1RewardTitle, broadcasterId: DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId));

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
