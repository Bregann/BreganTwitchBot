using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Services.Discord;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    /// <summary>
    /// The mute itself needs a live guild, so these cover the decision: whether a message
    /// should be acted on at all.
    /// </summary>
    [TestFixture]
    public class DiscordMessageModerationTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<IDiscordClientProvider> _discordClientProvider;
        private Mock<IDiscordHelperService> _discordHelperService;

        private DiscordMessageModerationService _moderationService;

        private const ulong GuildId = DatabaseSeedHelper.DiscordGuildId;
        private const ulong ChannelId = 998877;
        private const ulong UserId = DatabaseSeedHelper.DiscordUserId1;

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

            _discordClientProvider = new Mock<IDiscordClientProvider>();
            _discordHelperService = new Mock<IDiscordHelperService>();

            _moderationService = new DiscordMessageModerationService(
                _dbContext, _discordClientProvider.Object, _discordHelperService.Object);
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

        private Task<bool> Check(string message, bool isBot = false)
        {
            return _moderationService.CheckMessageAsync(GuildId, ChannelId, UserId, message, isBot);
        }

        [Test]
        public async Task CleanMessage_IsNotActedOn()
        {
            Assert.That(await Check("hello everybody how are we"), Is.False);
        }

        [Test]
        public async Task BotMessages_AreIgnored()
        {
            Assert.That(await Check(DatabaseSeedHelper.SeededChannel1BannedWord, isBot: true), Is.False);
        }

        [Test]
        public async Task EmptyMessage_IsIgnored()
        {
            Assert.That(await Check("   "), Is.False);
        }

        [Test]
        public async Task BlacklistedWord_IsActedOn()
        {
            Assert.That(await Check($"you are a {DatabaseSeedHelper.SeededChannel1BannedWord}"), Is.True);
        }

        [Test]
        public async Task BlacklistedWord_SplitWithPunctuation_IsStillCaught()
        {
            // the check strips anything that isn't a letter or digit, so spacing and
            // punctuation cannot be used to slip a word past it
            var spaced = string.Join(".", DatabaseSeedHelper.SeededChannel1BannedWord.ToCharArray());

            Assert.That(await Check(spaced), Is.True);
        }

        [Test]
        public async Task BlacklistedWord_InDifferentCase_IsStillCaught()
        {
            Assert.That(await Check(DatabaseSeedHelper.SeededChannel1BannedWord.ToUpper()), Is.True);
        }

        [Test]
        public async Task NitroScamLink_IsActedOn()
        {
            Assert.That(await Check("free nitro at https://discordnitro.xyz/claim"), Is.True);
        }

        [Test]
        public async Task GenuineDiscordNitroLink_IsAllowed()
        {
            // a real gift links to discord itself
            Assert.That(await Check("check out nitro at https://discord.com/gifts/abc123"), Is.False);
        }

        [Test]
        public async Task MentionOfNitroWithNoLink_IsAllowed()
        {
            Assert.That(await Check("i wish i had nitro"), Is.False);
        }

        [Test]
        public async Task UnknownGuild_IsIgnored()
        {
            var result = await _moderationService.CheckMessageAsync(
                99999999, ChannelId, UserId, DatabaseSeedHelper.SeededChannel1BannedWord, false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task WarnOnlyWords_AreNotActedOnInDiscord()
        {
            // only permanent ban words mute in discord, matching the old behaviour
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _dbContext.Blacklist.Add(new Blacklist
            {
                ChannelId = channel.Id,
                Word = "mildlyrude",
                WordType = WordType.StrikeWord
            });
            await _dbContext.SaveChangesAsync();

            Assert.That(await Check("that is mildlyrude"), Is.False);
        }

        [Test]
        public async Task BlacklistIsReadLive_SoWordsAddedInChatApplyImmediately()
        {
            // the old bot loaded a static list at startup, so a word banned in chat did not
            // apply in discord until a restart
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(await Check("freshlybannedword"), Is.False);

            _dbContext.Blacklist.Add(new Blacklist
            {
                ChannelId = channel.Id,
                Word = "freshlybannedword",
                WordType = WordType.PermBanWord
            });
            await _dbContext.SaveChangesAsync();

            Assert.That(await Check("freshlybannedword"), Is.True);
        }
    }
}
