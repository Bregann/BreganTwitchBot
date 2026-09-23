using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.GeneralCommands;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class UnbirthdayTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;

        private GeneralCommandsData _generalCommandsData;

        private const ulong GuildId = DatabaseSeedHelper.DiscordGuildId;

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
            config.DiscordGuildId = GuildId;
            await _dbContext.SaveChangesAsync();

            _generalCommandsData = new GeneralCommandsData(
                _dbContext,
                new Mock<IConfigHelperService>().Object,
                new Mock<IDiscordHelperService>().Object);
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

        private async Task AddBirthdayFor(ulong discordUserId)
        {
            await _generalCommandsData.AddUserBirthday(new AddBirthdayCommand
            {
                GuildId = GuildId,
                UserId = discordUserId,
                ChannelId = 1,
                Day = 5,
                Month = 6
            });
        }

        [Test]
        public async Task Unbirthday_RemovesTheBirthday()
        {
            await AddBirthdayFor(DatabaseSeedHelper.DiscordUserId1);

            var response = await _generalCommandsData.RemoveUserBirthday(GuildId, DatabaseSeedHelper.DiscordUserId1);

            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var stillThere = await _dbContext.Birthdays.AnyAsync(x => x.User.DiscordUserId == DatabaseSeedHelper.DiscordUserId1 && x.ChannelId == channel.Id);

            Assert.Multiple(() =>
            {
                Assert.That(stillThere, Is.False);
                Assert.That(response, Does.Contain("has been removed"));
            });
        }

        [Test]
        public async Task Unbirthday_WithNoBirthdaySet_SaysSo()
        {
            var response = await _generalCommandsData.RemoveUserBirthday(GuildId, DatabaseSeedHelper.DiscordUserId1);

            Assert.That(response, Does.Contain("don't have a birthday set"));
        }

        [Test]
        public async Task Unbirthday_OnlyRemovesTheCallersOwn()
        {
            await AddBirthdayFor(DatabaseSeedHelper.DiscordUserId1);
            await AddBirthdayFor(DatabaseSeedHelper.DiscordUserId2);

            await _generalCommandsData.RemoveUserBirthday(GuildId, DatabaseSeedHelper.DiscordUserId1);

            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var othersBirthday = await _dbContext.Birthdays.AnyAsync(x => x.User.DiscordUserId == DatabaseSeedHelper.DiscordUserId2 && x.ChannelId == channel.Id);

            Assert.That(othersBirthday, Is.True, "the other user's birthday should be untouched");
        }

        [Test]
        public async Task AddingABirthdayAgainAfterRemoving_IsAllowed()
        {
            await AddBirthdayFor(DatabaseSeedHelper.DiscordUserId1);
            await _generalCommandsData.RemoveUserBirthday(GuildId, DatabaseSeedHelper.DiscordUserId1);

            var response = await _generalCommandsData.AddUserBirthday(new AddBirthdayCommand
            {
                GuildId = GuildId,
                UserId = DatabaseSeedHelper.DiscordUserId1,
                ChannelId = 1,
                Day = 1,
                Month = 1
            });

            Assert.That(response, Does.Not.Contain("already have a birthday"));
        }
    }
}
