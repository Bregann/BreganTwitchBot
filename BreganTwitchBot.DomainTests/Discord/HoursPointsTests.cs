using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Services.Discord.SlashCommands.HoursPoints;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class HoursPointsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<IDiscordHelperService> _discordHelperService;

        private DiscordHoursPointsData _hoursPointsData;

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

            _discordHelperService = new Mock<IDiscordHelperService>();
            _hoursPointsData = new DiscordHoursPointsData(_dbContext, _discordHelperService.Object);
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

        private static HoursPointsCommand CreateCommand(string? twitchUsername = null, ulong? discordUserId = null, ulong caller = DatabaseSeedHelper.DiscordUserId1)
        {
            return new HoursPointsCommand
            {
                GuildId = DatabaseSeedHelper.DiscordGuildId,
                ChannelId = 12345,
                CallerDiscordUserId = caller,
                DiscordUserId = discordUserId,
                TwitchUsername = twitchUsername
            };
        }

        [Test]
        public async Task Hours_ByTwitchUsername_ReturnsWatchtime()
        {
            var result = await _hoursPointsData.HandleHoursCommand(CreateCommand(twitchUsername: DatabaseSeedHelper.Channel1User1TwitchUsername));

            Assert.Multiple(() =>
            {
                Assert.That(result.Title, Does.Contain(DatabaseSeedHelper.Channel1User1TwitchUsername));
                Assert.That(result.Fields, Does.ContainKey("Time watched"));
            });
        }

        [Test]
        public async Task Hours_ForTheCaller_ReturnsTheirWatchtime()
        {
            var result = await _hoursPointsData.HandleHoursCommand(CreateCommand());

            Assert.That(result.Fields, Does.ContainKey("Time watched"));
        }

        [Test]
        public async Task Hours_UserWithZeroWatchtime_StillReportsThem()
        {
            // the old bot treated 0 minutes as "user does not exist"
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var watchtime = await _dbContext.ChannelUserWatchtime.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            if (watchtime != null)
            {
                watchtime.MinutesInStream = 0;
                await _dbContext.SaveChangesAsync();
            }

            var result = await _hoursPointsData.HandleHoursCommand(CreateCommand(twitchUsername: DatabaseSeedHelper.Channel1User1TwitchUsername));

            Assert.Multiple(() =>
            {
                Assert.That(result.Fields["Time watched"], Does.Contain("0 minutes"));
                Assert.That(result.Description, Does.Not.Contain("does not exist"));
            });
        }

        [Test]
        public async Task Hours_UnknownTwitchUsername_SaysSo()
        {
            var result = await _hoursPointsData.HandleHoursCommand(CreateCommand(twitchUsername: "someonewhodoesnotexist"));

            Assert.That(result.Description, Does.Contain("don't know anybody"));
        }

        [Test]
        public async Task Points_ByTwitchUsername_ReturnsPointsUnderCurrencyName()
        {
            var result = await _hoursPointsData.HandlePointsCommand(CreateCommand(twitchUsername: DatabaseSeedHelper.Channel1User1TwitchUsername));

            Assert.Multiple(() =>
            {
                Assert.That(result.Fields, Does.ContainKey(DatabaseSeedHelper.Channel1ChannelCurrencyName));
                Assert.That(result.Title, Does.Contain(DatabaseSeedHelper.Channel1ChannelCurrencyName));
            });
        }

        [Test]
        public async Task Points_UserWithZeroPoints_StillReportsThem()
        {
            // the old bot treated 0 points as "user does not exist"
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var userData = await _dbContext.ChannelUserData.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);
            userData.Points = 0;
            await _dbContext.SaveChangesAsync();

            var result = await _hoursPointsData.HandlePointsCommand(CreateCommand(twitchUsername: DatabaseSeedHelper.Channel1User1TwitchUsername));

            Assert.Multiple(() =>
            {
                Assert.That(result.Fields[DatabaseSeedHelper.Channel1ChannelCurrencyName], Is.EqualTo("0"));
                Assert.That(result.Description, Does.Not.Contain("does not exist"));
            });
        }

        /// <summary>
        /// Discord stats aren't seeded by default, so prestige tests create the row first
        /// </summary>
        private async Task<(Domain.Database.Models.ChannelUser User, Domain.Database.Models.Channel Channel)> GiveCallerPointsAndDiscordStats(long points)
        {
            var user = await _dbContext.ChannelUsers.FirstAsync(x => x.DiscordUserId == DatabaseSeedHelper.DiscordUserId1);
            var channel = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var userData = await _dbContext.ChannelUserData.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);
            userData.Points = points;

            if (!await _dbContext.DiscordUserStats.AnyAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id))
            {
                await _dbContext.DiscordUserStats.AddAsync(new Domain.Database.Models.DiscordUserStats
                {
                    ChannelUserId = user.Id,
                    ChannelId = channel.Id,
                    DiscordLevel = 1,
                    DiscordXp = 0,
                    DiscordLevelUpNotifsEnabled = true,
                    PrestigeLevel = 0
                });
            }

            await _dbContext.SaveChangesAsync();
            return (user, channel);
        }

        [Test]
        public async Task Prestige_WithoutEnoughPoints_IsRefused()
        {
            var result = await _hoursPointsData.HandlePrestigeCommand(CreateCommand());

            Assert.That(result, Does.Contain("don't have enough"));
        }

        [Test]
        public async Task Prestige_WithEnoughPoints_IncrementsAndSpends()
        {
            var channelForCap = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var cap = channelForCap.ChannelConfig.CurrencyPointCap;

            var (user, channel) = await GiveCallerPointsAndDiscordStats(cap + 500);
            var userData = await _dbContext.ChannelUserData.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            var result = await _hoursPointsData.HandlePrestigeCommand(CreateCommand());

            var discordStats = await _dbContext.DiscordUserStats.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("You have prestiged"));
                Assert.That(userData.Points, Is.EqualTo(500));
                Assert.That(discordStats.PrestigeLevel, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task Prestige_AwardsDiscordXp()
        {
            var channelForCap = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            await GiveCallerPointsAndDiscordStats(channelForCap.ChannelConfig.CurrencyPointCap);

            await _hoursPointsData.HandlePrestigeCommand(CreateCommand());

            _discordHelperService.Verify(x => x.AddDiscordXpToUser(DatabaseSeedHelper.DiscordGuildId, It.IsAny<ulong>(), DatabaseSeedHelper.DiscordUserId1, 7500), Times.Once);
        }

        [Test]
        public async Task Prestige_UnlinkedUser_IsToldToLink()
        {
            var result = await _hoursPointsData.HandlePrestigeCommand(CreateCommand(caller: DatabaseSeedHelper.DiscordUserNonExistentId));

            Assert.That(result, Does.Contain("link your Twitch account"));
        }

        [Test]
        public async Task UnknownGuild_SaysNotLinked()
        {
            var command = CreateCommand(twitchUsername: DatabaseSeedHelper.Channel1User1TwitchUsername);
            command.GuildId = 9999999;

            var result = await _hoursPointsData.HandleHoursCommand(command);

            Assert.That(result.Description, Does.Contain("isn't linked to a channel"));
        }
    }
}
