using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.SelfAssignRoles;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class SelfAssignRoleTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<IDiscordClientProvider> _discordClientProvider;

        private DiscordSelfAssignRoleData _selfAssignRoleData;

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
            _selfAssignRoleData = new DiscordSelfAssignRoleData(_dbContext, _discordClientProvider.Object);
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

        private async Task SeedRoles()
        {
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            await _dbContext.DiscordSelfAssignRoles.AddRangeAsync(
                new DiscordSelfAssignRole { ChannelId = channel.Id, DiscordRoleId = 111, DisplayName = "Weeb", SortOrder = 2 },
                new DiscordSelfAssignRole { ChannelId = channel.Id, DiscordRoleId = 222, DisplayName = "Marbles On Stream", Emoji = "🔵", SortOrder = 1 });

            await _dbContext.SaveChangesAsync();
        }

        [Test]
        public async Task GetRoles_ReturnsRolesInSortOrder()
        {
            await SeedRoles();

            var roles = await _selfAssignRoleData.GetRoles(DatabaseSeedHelper.DiscordGuildId);

            Assert.Multiple(() =>
            {
                Assert.That(roles, Has.Count.EqualTo(2));
                Assert.That(roles[0].DisplayName, Is.EqualTo("Marbles On Stream"));
                Assert.That(roles[1].DisplayName, Is.EqualTo("Weeb"));
            });
        }

        [Test]
        public async Task GetRoles_UnknownGuild_ReturnsEmpty()
        {
            await SeedRoles();

            var roles = await _selfAssignRoleData.GetRoles(99999999);

            Assert.That(roles, Is.Empty);
        }

        [Test]
        public async Task GetRoles_NoneConfigured_ReturnsEmpty()
        {
            var roles = await _selfAssignRoleData.GetRoles(DatabaseSeedHelper.DiscordGuildId);

            Assert.That(roles, Is.Empty);
        }

        [Test]
        public async Task ToggleRole_UnknownRoleConfig_IsRejected()
        {
            var (response, ephemeral) = await _selfAssignRoleData.ToggleRole(DatabaseSeedHelper.DiscordGuildId, DatabaseSeedHelper.DiscordUserId1, 99999);

            Assert.Multiple(() =>
            {
                Assert.That(response, Does.Contain("isn't available"));
                Assert.That(ephemeral, Is.True);
            });
        }

        [Test]
        public async Task ToggleRole_FromAnotherGuild_IsRejected()
        {
            await SeedRoles();
            var role = await _dbContext.DiscordSelfAssignRoles.FirstAsync();

            // the role belongs to this guild's channel, so asking as a different guild must fail
            var (response, _) = await _selfAssignRoleData.ToggleRole(99999999, DatabaseSeedHelper.DiscordUserId1, role.Id);

            Assert.That(response, Does.Contain("isn't available"));
        }

        [Test]
        public async Task RolesAreStoredPerChannel()
        {
            await SeedRoles();

            var channel2 = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);
            var channel2Roles = await _dbContext.DiscordSelfAssignRoles.CountAsync(x => x.ChannelId == channel2.Id);

            Assert.That(channel2Roles, Is.EqualTo(0));
        }
    }
}
