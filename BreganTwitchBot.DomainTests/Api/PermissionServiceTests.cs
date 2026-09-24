using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Services.Api;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Api
{
    [TestFixture]
    public class PermissionServiceTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private PermissionService _permissionService;

        private const string Channel = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName;
        private const string BroadcasterId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId;
        private const string ModUsername = DatabaseSeedHelper.Channel1User1TwitchUsername;
        private const string ModId = DatabaseSeedHelper.Channel1User1TwitchUserId;

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

            _permissionService = new PermissionService(_dbContext);
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
        public async Task Broadcaster_HasEveryPermissionWithoutBeingGrantedAny()
        {
            foreach (var permission in Enum.GetValues<ChannelPermission>())
            {
                var has = await _permissionService.HasPermissionAsync(Channel, BroadcasterId, permission);
                Assert.That(has, Is.True, $"the broadcaster should have {permission}");
            }
        }

        [Test]
        public async Task UngrantedUser_HasNoPermissions()
        {
            var has = await _permissionService.HasPermissionAsync(Channel, ModId, ChannelPermission.EditCommands);

            Assert.That(has, Is.False);
        }

        [Test]
        public async Task GrantedPermission_IsHeld()
        {
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands]);

            var has = await _permissionService.HasPermissionAsync(Channel, ModId, ChannelPermission.EditCommands);

            Assert.That(has, Is.True);
        }

        [Test]
        public async Task GrantingOnePermission_DoesNotGrantOthers()
        {
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands]);

            var hasSubathon = await _permissionService.HasPermissionAsync(Channel, ModId, ChannelPermission.EditSubathon);

            Assert.That(hasSubathon, Is.False);
        }

        [Test]
        public async Task GrantingAnyEditPermission_AlsoGrantsViewAdmin()
        {
            // without it the grant would appear applied but the user could not reach the page
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands]);

            var canView = await _permissionService.HasPermissionAsync(Channel, ModId, ChannelPermission.ViewAdmin);

            Assert.That(canView, Is.True);
        }

        [Test]
        public async Task SetPermissions_ReplacesRatherThanAdds()
        {
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands, ChannelPermission.EditSubathon]);
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands]);

            var hasSubathon = await _permissionService.HasPermissionAsync(Channel, ModId, ChannelPermission.EditSubathon);

            Assert.That(hasSubathon, Is.False, "the removed permission should be gone");
        }

        [Test]
        public async Task SetPermissions_WithAnEmptyList_RevokesEverything()
        {
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands]);
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, []);

            var canView = await _permissionService.HasPermissionAsync(Channel, ModId, ChannelPermission.ViewAdmin);

            Assert.That(canView, Is.False);
        }

        [Test]
        public void ModCannotGrantPermissions()
        {
            // the escalation guard - a mod must not be able to give themselves more
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _permissionService.SetPermissionsAsync(Channel, ModId, ModUsername, [ChannelPermission.EditChannelConfig]));
        }

        [Test]
        public async Task ModWithEveryOtherPermission_StillCannotGrantPermissions()
        {
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, Enum.GetValues<ChannelPermission>().ToList());

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _permissionService.SetPermissionsAsync(Channel, ModId, DatabaseSeedHelper.Channel1User2TwitchUsername, [ChannelPermission.EditCommands]));
        }

        [Test]
        public void SetPermissions_ForAnUnknownUser_IsRejected()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, "somebodywhodoesnotexist", [ChannelPermission.EditCommands]));
        }

        [Test]
        public async Task PermissionsAreScopedToTheirChannel()
        {
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands]);

            var hasInOtherChannel = await _permissionService.HasPermissionAsync(
                DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName, ModId, ChannelPermission.EditCommands);

            Assert.That(hasInOtherChannel, Is.False);
        }

        [Test]
        public async Task IsBroadcaster_OnlyForTheirOwnChannel()
        {
            Assert.Multiple(async () =>
            {
                Assert.That(await _permissionService.IsBroadcasterAsync(Channel, BroadcasterId), Is.True);
                Assert.That(await _permissionService.IsBroadcasterAsync(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName, BroadcasterId), Is.False);
                Assert.That(await _permissionService.IsBroadcasterAsync(Channel, ModId), Is.False);
            });
        }

        [Test]
        public async Task GetPermissions_ListsHoldersAndWhoGrantedThem()
        {
            await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, ModUsername, [ChannelPermission.EditCommands]);

            var holders = await _permissionService.GetPermissionsAsync(Channel);

            Assert.Multiple(() =>
            {
                Assert.That(holders, Has.Count.EqualTo(1));
                Assert.That(holders![0].TwitchUsername, Is.EqualTo(ModUsername));
                Assert.That(holders[0].GrantedByTwitchUserId, Is.EqualTo(BroadcasterId));
                Assert.That(holders[0].Permissions, Does.Contain(ChannelPermission.EditCommands));
            });
        }

        [Test]
        public async Task GrantingToTheBroadcaster_IsRejected()
        {
            // the broadcaster only gets a ChannelUser row once they have chatted in
            // their own channel, so create one to reach the guard
            _dbContext.ChannelUsers.Add(new Domain.Database.Models.ChannelUser
            {
                TwitchUserId = BroadcasterId,
                TwitchUsername = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                DiscordUserId = 0,
                AddedOn = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                CanUseOpenAi = false
            });
            await _dbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _permissionService.SetPermissionsAsync(Channel, BroadcasterId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, [ChannelPermission.EditCommands]));
        }

        [Test]
        public async Task UnknownChannel_GrantsNothing()
        {
            var has = await _permissionService.HasPermissionAsync("notachannel", BroadcasterId, ChannelPermission.ViewAdmin);

            Assert.That(has, Is.False);
        }
    }
}
