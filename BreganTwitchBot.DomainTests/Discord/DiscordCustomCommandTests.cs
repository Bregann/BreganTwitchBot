using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Services.Discord;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class DiscordCustomCommandTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private DiscordCustomCommandService _customCommandService;

        private const ulong GuildId = DatabaseSeedHelper.DiscordGuildId;
        private const ulong CommandsChannelId = 555000;
        private const string Username = "someviewer";

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

            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            config.DiscordUserCommandsChannelId = CommandsChannelId;
            await _dbContext.SaveChangesAsync();

            _customCommandService = new DiscordCustomCommandService(_dbContext);
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

        private Task<string?> Handle(string message, ulong channelId = CommandsChannelId, bool isMod = false)
        {
            return _customCommandService.TryHandleCustomCommand(GuildId, channelId, Username, message, isMod);
        }

        [Test]
        public async Task KnownCommand_IsAnswered()
        {
            var reply = await Handle("!readytouse");

            Assert.That(reply, Is.Not.Null);
        }

        [Test]
        public async Task UnknownCommand_IsIgnored()
        {
            Assert.That(await Handle("!notacommand"), Is.Null);
        }

        [Test]
        public async Task OrdinaryMessage_IsIgnored()
        {
            Assert.That(await Handle("just chatting away"), Is.Null);
        }

        [Test]
        public async Task CommandIsMatchedCaseInsensitively()
        {
            Assert.That(await Handle("!READYTOUSE"), Is.Not.Null);
        }

        [Test]
        public async Task CommandWithTrailingText_StillMatches()
        {
            Assert.That(await Handle("!readytouse and some extra words"), Is.Not.Null);
        }

        [Test]
        public async Task OutsideTheCommandsChannel_IsIgnoredForNonMods()
        {
            Assert.That(await Handle("!readytouse", channelId: 999111), Is.Null);
        }

        [Test]
        public async Task OutsideTheCommandsChannel_IsAllowedForMods()
        {
            Assert.That(await Handle("!readytouse", channelId: 999111, isMod: true), Is.Not.Null);
        }

        [Test]
        public async Task AnotherChannelsCommand_IsNotAnswered()
        {
            // !2ndchannel belongs to channel 2, so this guild must not answer it
            Assert.That(await Handle("!2ndchannel"), Is.Null);
        }

        [Test]
        public async Task UnknownGuild_IsIgnored()
        {
            var reply = await _customCommandService.TryHandleCustomCommand(99999999, CommandsChannelId, Username, "!readytouse", false);

            Assert.That(reply, Is.Null);
        }

        [Test]
        public async Task UsingACommand_IncrementsItsCount()
        {
            var before = (await _dbContext.CustomCommands.FirstAsync(x => x.CommandName == "!readytouse")).TimesUsed;

            await Handle("!readytouse");

            var after = (await _dbContext.CustomCommands.FirstAsync(x => x.CommandName == "!readytouse")).TimesUsed;

            Assert.That(after, Is.EqualTo(before + 1));
        }

        [Test]
        public async Task UserPlaceholder_IsFilledIn()
        {
            // the old bot skipped commands using placeholders as it had nothing to put in them
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _dbContext.CustomCommands.Add(new Domain.Database.Models.CustomCommand
            {
                ChannelId = channel.Id,
                CommandName = "!greet",
                CommandText = "hello [user]",
                LastUsed = DateTime.UtcNow,
                TimesUsed = 0
            });
            await _dbContext.SaveChangesAsync();

            var reply = await Handle("!greet");

            Assert.That(reply, Is.EqualTo($"hello @{Username}"));
        }

        [Test]
        public async Task CountPlaceholder_IsFilledIn()
        {
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _dbContext.CustomCommands.Add(new Domain.Database.Models.CustomCommand
            {
                ChannelId = channel.Id,
                CommandName = "!counter",
                CommandText = "used [count] times",
                LastUsed = DateTime.UtcNow,
                TimesUsed = 4
            });
            await _dbContext.SaveChangesAsync();

            var reply = await Handle("!counter");

            // the count is incremented before the reply is built, so it reads 5
            Assert.That(reply, Is.EqualTo("used 5 times"));
        }
    }
}
