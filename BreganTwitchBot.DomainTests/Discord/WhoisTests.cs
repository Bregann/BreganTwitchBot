using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.GeneralCommands;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class WhoisTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private DiscordWhoisData _whoisData;

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

            _whoisData = new DiscordWhoisData(_dbContext);
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
        public async Task Whois_ByTwitchUsername_ReturnsTheUser()
        {
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                TwitchUsername = DatabaseSeedHelper.Channel1User1TwitchUsername
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.Fields["Twitch name"], Is.EqualTo(DatabaseSeedHelper.Channel1User1TwitchUsername));
                Assert.That(result.Fields, Does.ContainKey("Last seen"));
                Assert.That(result.Fields, Does.ContainKey("Total messages"));
            });
        }

        [Test]
        public async Task Whois_ByDiscordUser_ReturnsTheUser()
        {
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                DiscordUserId = DatabaseSeedHelper.DiscordUserId1
            });

            Assert.That(result.Fields, Does.ContainKey("Twitch name"));
        }

        [Test]
        public async Task Whois_TwitchUsernameIsCaseInsensitive()
        {
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                TwitchUsername = DatabaseSeedHelper.Channel1User1TwitchUsername.ToUpper()
            });

            Assert.That(result.Fields["Twitch name"], Is.EqualTo(DatabaseSeedHelper.Channel1User1TwitchUsername));
        }

        [Test]
        public async Task Whois_UnknownTwitchUsername_SaysNoAccountFound()
        {
            // the old bot answered this with a random joke name from a hardcoded list, which
            // looked like a real answer
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                TwitchUsername = "someonewhodoesnotexist"
            });

            Assert.Multiple(() =>
            {
                Assert.That(result.Fields["Result"], Is.EqualTo("No linked account found"));
                Assert.That(result.Fields["Looked up"], Is.EqualTo("someonewhodoesnotexist"));
            });
        }

        [Test]
        public async Task Whois_UnknownDiscordUser_SaysNoAccountFound()
        {
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                DiscordUserId = DatabaseSeedHelper.DiscordUserNonExistentId
            });

            Assert.That(result.Fields["Result"], Is.EqualTo("No linked account found"));
        }

        [Test]
        public async Task Whois_NothingSupplied_AsksForSomething()
        {
            // the old bot returned null here, which the module would have dereferenced
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId
            });

            Assert.That(result.Title, Does.Contain("Give me either"));
        }

        [Test]
        public async Task Whois_UnknownGuild_SaysNotLinked()
        {
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = 99999999,
                TwitchUsername = DatabaseSeedHelper.Channel1User1TwitchUsername
            });

            Assert.That(result.Title, Does.Contain("isn't linked to a channel"));
        }

        [Test]
        public async Task Whois_UserWithNoDiscordLink_ShowsNotLinked()
        {
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User2TwitchUserId);
            user.DiscordUserId = 0;
            await _dbContext.SaveChangesAsync();

            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                TwitchUsername = DatabaseSeedHelper.Channel1User2TwitchUsername
            });

            Assert.That(result.Fields["Discord name"], Is.EqualTo("Not linked"));
        }

        [Test]
        public async Task Whois_IncludesPointsUnderTheChannelCurrencyName()
        {
            var result = await _whoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                TwitchUsername = DatabaseSeedHelper.Channel1User1TwitchUsername
            });

            Assert.That(result.Fields, Does.ContainKey(DatabaseSeedHelper.Channel1ChannelCurrencyName));
        }
    }
}
