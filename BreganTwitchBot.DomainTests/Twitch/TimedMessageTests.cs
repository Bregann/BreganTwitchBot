using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch
{
    [TestFixture]
    public class TimedMessageTests
    {
        private PostgreSqlContainer _postgresContainer;
        private ServiceProvider _serviceProvider;
        private AppDbContext _dbContext;
        private Mock<ITwitchHelperService> _twitchHelperService;

        private TimedMessageService _timedMessageService;

        private const string ChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;

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

            _twitchHelperService = new Mock<ITwitchHelperService>();
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(true);
            _twitchHelperService.Setup(x => x.GetChatMessageCount(It.IsAny<string>())).Returns(1000);

            _timedMessageService = new TimedMessageService(_serviceProvider, _twitchHelperService.Object);
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

        private async Task<ChannelTimedMessage> AddMessage(
            int intervalMinutes = 30,
            int minimumChatMessages = 0,
            bool enabled = true,
            bool onlyWhenLive = true,
            DateTime? lastSentAt = null)
        {
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == ChannelId);

            var message = new ChannelTimedMessage
            {
                ChannelId = channel.Id,
                Message = "join the discord",
                IntervalMinutes = intervalMinutes,
                MinimumChatMessages = minimumChatMessages,
                Enabled = enabled,
                OnlyWhenLive = onlyWhenLive,
                LastSentAt = lastSentAt
            };

            _dbContext.ChannelTimedMessages.Add(message);
            await _dbContext.SaveChangesAsync();

            return message;
        }

        private void VerifySent(Times times)
        {
            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                ChannelId, It.IsAny<string>(), "join the discord", null), times);
        }

        [Test]
        public async Task NeverSentMessage_IsSent()
        {
            await AddMessage();

            await _timedMessageService.SendDueMessages();

            VerifySent(Times.Once());
        }

        [Test]
        public async Task MessageSentRecently_IsNotSentAgain()
        {
            await AddMessage(intervalMinutes: 30, lastSentAt: DateTime.UtcNow.AddMinutes(-5));

            await _timedMessageService.SendDueMessages();

            VerifySent(Times.Never());
        }

        [Test]
        public async Task MessageDue_IsSentAgain()
        {
            await AddMessage(intervalMinutes: 30, lastSentAt: DateTime.UtcNow.AddMinutes(-31));

            await _timedMessageService.SendDueMessages();

            VerifySent(Times.Once());
        }

        [Test]
        public async Task DisabledMessage_IsNotSent()
        {
            await AddMessage(enabled: false);

            await _timedMessageService.SendDueMessages();

            VerifySent(Times.Never());
        }

        [Test]
        public async Task LiveOnlyMessage_IsNotSentWhenOffline()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(false);
            await AddMessage(onlyWhenLive: true);

            await _timedMessageService.SendDueMessages();

            VerifySent(Times.Never());
        }

        [Test]
        public async Task MessageNotRestrictedToLive_IsSentWhenOffline()
        {
            _twitchHelperService.Setup(x => x.IsBroadcasterLive(It.IsAny<string>())).ReturnsAsync(false);
            await AddMessage(onlyWhenLive: false);

            await _timedMessageService.SendDueMessages();

            VerifySent(Times.Once());
        }

        [Test]
        public async Task SendingUpdatesLastSentAt()
        {
            var message = await AddMessage();

            await _timedMessageService.SendDueMessages();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var reloaded = await context.ChannelTimedMessages.FirstAsync(x => x.Id == message.Id);

            Assert.That(reloaded.LastSentAt, Is.Not.Null);
        }

        [Test]
        public async Task QuietChat_HoldsTheMessageBack()
        {
            // the bot should not talk to itself in a dead chat
            _twitchHelperService.Setup(x => x.GetChatMessageCount(It.IsAny<string>())).Returns(2);

            await AddMessage(minimumChatMessages: 50);
            await _timedMessageService.SendDueMessages();
            VerifySent(Times.Once());

            // second run: only two more messages have gone by, so it should hold
            _twitchHelperService.Setup(x => x.GetChatMessageCount(It.IsAny<string>())).Returns(4);

            var message = await _dbContext.ChannelTimedMessages.FirstAsync();
            message.LastSentAt = DateTime.UtcNow.AddHours(-1);
            await _dbContext.SaveChangesAsync();

            await _timedMessageService.SendDueMessages();
            VerifySent(Times.Once());
        }

        [Test]
        public async Task BusyChat_LetsTheMessageThrough()
        {
            _twitchHelperService.Setup(x => x.GetChatMessageCount(It.IsAny<string>())).Returns(0);

            await AddMessage(minimumChatMessages: 50);
            await _timedMessageService.SendDueMessages();
            VerifySent(Times.Once());

            _twitchHelperService.Setup(x => x.GetChatMessageCount(It.IsAny<string>())).Returns(100);

            var message = await _dbContext.ChannelTimedMessages.FirstAsync();
            message.LastSentAt = DateTime.UtcNow.AddHours(-1);
            await _dbContext.SaveChangesAsync();

            await _timedMessageService.SendDueMessages();
            VerifySent(Times.Exactly(2));
        }

        [Test]
        public async Task MessagesAreScopedToTheirChannel()
        {
            await AddMessage();

            await _timedMessageService.SendDueMessages();

            _twitchHelperService.Verify(x => x.SendTwitchMessageToChannel(
                DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
                Times.Never());
        }
    }
}
