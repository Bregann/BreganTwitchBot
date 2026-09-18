using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Api;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Api
{
    [TestFixture]
    public class AdminDataServiceTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<ICommandHandler> _commandHandler;

        private AdminDataService _adminDataService;

        private const string Channel = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName;

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

            _commandHandler = new Mock<ICommandHandler>();
            _adminDataService = new AdminDataService(_dbContext, _commandHandler.Object);
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

        // Channel config

        [Test]
        public async Task UpdateChannelConfig_SavesTheValues()
        {
            await _adminDataService.UpdateChannelConfigAsync(Channel, new UpdateChannelConfigRequest
            {
                ChannelCurrencyName = "Doubloons",
                CurrencyPointCap = 500000
            });

            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelName == Channel);

            Assert.Multiple(() =>
            {
                Assert.That(config.ChannelCurrencyName, Is.EqualTo("Doubloons"));
                Assert.That(config.CurrencyPointCap, Is.EqualTo(500000));
            });
        }

        [Test]
        public void UpdateChannelConfig_WithAnEmptyCurrencyName_IsRejected()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _adminDataService.UpdateChannelConfigAsync(Channel, new UpdateChannelConfigRequest
                {
                    ChannelCurrencyName = "  ",
                    CurrencyPointCap = 1000
                }));
        }

        [Test]
        public void UpdateChannelConfig_WithANonPositiveCap_IsRejected()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _adminDataService.UpdateChannelConfigAsync(Channel, new UpdateChannelConfigRequest
                {
                    ChannelCurrencyName = "Points",
                    CurrencyPointCap = 0
                }));
        }

        [Test]
        public void UpdateChannelConfig_ForAnUnknownChannel_IsRejected()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _adminDataService.UpdateChannelConfigAsync("notachannel", new UpdateChannelConfigRequest
                {
                    ChannelCurrencyName = "Points",
                    CurrencyPointCap = 1000
                }));
        }

        // Custom commands

        [Test]
        public async Task UpsertCustomCommand_AddsANewCommand()
        {
            await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest
            {
                CommandName = "discord",
                CommandText = "Join at example.com"
            });

            var command = await _dbContext.CustomCommands.FirstOrDefaultAsync(x => x.CommandName == "!discord");

            Assert.Multiple(() =>
            {
                Assert.That(command, Is.Not.Null);
                Assert.That(command!.CommandText, Is.EqualTo("Join at example.com"));
            });
        }

        [Test]
        public async Task UpsertCustomCommand_AddsTheLeadingExclamationMark()
        {
            await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest
            {
                CommandName = "socials",
                CommandText = "links"
            });

            Assert.That(await _dbContext.CustomCommands.AnyAsync(x => x.CommandName == "!socials"), Is.True);
        }

        [Test]
        public async Task UpsertCustomCommand_UpdatesAnExistingCommand()
        {
            await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest { CommandName = "!twitter", CommandText = "old" });
            await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest { CommandName = "!twitter", CommandText = "new" });

            var commands = await _dbContext.CustomCommands.Where(x => x.CommandName == "!twitter").ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(commands, Has.Count.EqualTo(1), "it should update rather than add a duplicate");
                Assert.That(commands[0].CommandText, Is.EqualTo("new"));
            });
        }

        [Test]
        public void UpsertCustomCommand_ShadowingABuiltInCommand_IsRejected()
        {
            // a custom command that shadows a built in one would never fire
            _commandHandler.Setup(x => x.IsSystemCommand(It.IsAny<string>())).Returns(true);

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest
                {
                    CommandName = "points",
                    CommandText = "nope"
                }));
        }

        [Test]
        public void UpsertCustomCommand_WithNoResponse_IsRejected()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest
                {
                    CommandName = "empty",
                    CommandText = "   "
                }));
        }

        [Test]
        public async Task DeleteCustomCommand_RemovesIt()
        {
            await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest { CommandName = "!temp", CommandText = "text" });
            await _adminDataService.DeleteCustomCommandAsync(Channel, "!temp");

            Assert.That(await _dbContext.CustomCommands.AnyAsync(x => x.CommandName == "!temp"), Is.False);
        }

        [Test]
        public void DeleteCustomCommand_ThatDoesNotExist_IsRejected()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _adminDataService.DeleteCustomCommandAsync(Channel, "!nothere"));
        }

        [Test]
        public async Task CustomCommands_AreScopedToTheirChannel()
        {
            await _adminDataService.UpsertCustomCommandAsync(Channel, new UpsertCustomCommandRequest { CommandName = "!mine", CommandText = "text" });

            var channel2 = await _dbContext.Channels.FirstAsync(x => x.BroadcasterTwitchChannelName == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName);

            Assert.That(await _dbContext.CustomCommands.AnyAsync(x => x.CommandName == "!mine" && x.ChannelId == channel2.Id), Is.False);
        }

        // Ranks

        [Test]
        public async Task UpsertRank_AddsANewRank()
        {
            await _adminDataService.UpsertRankAsync(Channel, new UpsertChannelRankRequest
            {
                RankName = "Regular",
                RankMinutesRequired = 600,
                BonusRankPointsEarned = 5000
            });

            Assert.That(await _dbContext.ChannelRanks.AnyAsync(x => x.RankName == "Regular"), Is.True);
        }

        [Test]
        public async Task UpsertRank_UpdatesAnExistingRank()
        {
            var existing = await _dbContext.ChannelRanks.FirstAsync();

            await _adminDataService.UpsertRankAsync(Channel, new UpsertChannelRankRequest
            {
                Id = existing.Id,
                RankName = "Renamed",
                RankMinutesRequired = existing.RankMinutesRequired,
                BonusRankPointsEarned = existing.BonusRankPointsEarned
            });

            var reloaded = await _dbContext.ChannelRanks.FirstAsync(x => x.Id == existing.Id);
            Assert.That(reloaded.RankName, Is.EqualTo("Renamed"));
        }

        [Test]
        public void UpsertRank_WithNegativeMinutes_IsRejected()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _adminDataService.UpsertRankAsync(Channel, new UpsertChannelRankRequest
                {
                    RankName = "Impossible",
                    RankMinutesRequired = -1,
                    BonusRankPointsEarned = 0
                }));
        }

        [Test]
        public void UpsertRank_ForARankInAnotherChannel_IsRejected()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _adminDataService.UpsertRankAsync(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName, new UpsertChannelRankRequest
                {
                    Id = _dbContext.ChannelRanks.First().Id,
                    RankName = "Stolen",
                    RankMinutesRequired = 100,
                    BonusRankPointsEarned = 0
                }));
        }

        [Test]
        public async Task DeleteRank_AlsoRemovesTheProgressPointingAtIt()
        {
            var rank = await _dbContext.ChannelRanks.FirstAsync();

            await _adminDataService.DeleteRankAsync(Channel, rank.Id);

            Assert.Multiple(async () =>
            {
                Assert.That(await _dbContext.ChannelRanks.AnyAsync(x => x.Id == rank.Id), Is.False);
                Assert.That(await _dbContext.ChannelUserRankProgress.AnyAsync(x => x.ChannelRankId == rank.Id), Is.False);
            });
        }

        // Blacklist

        [Test]
        public async Task AddBlacklistWord_AddsIt()
        {
            await _adminDataService.AddBlacklistWordAsync(Channel, new AddBlacklistWordRequest
            {
                Word = "NaughtyWord",
                WordType = WordType.PermBanWord
            });

            // stored lowercase so matching is consistent
            Assert.That(await _dbContext.Blacklist.AnyAsync(x => x.Word == "naughtyword"), Is.True);
        }

        [Test]
        public void AddBlacklistWord_Duplicate_IsRejected()
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _adminDataService.AddBlacklistWordAsync(Channel, new AddBlacklistWordRequest
                {
                    Word = DatabaseSeedHelper.SeededChannel1BannedWord,
                    WordType = WordType.PermBanWord
                }));
        }

        [Test]
        public async Task DeleteBlacklistWord_RemovesIt()
        {
            var word = await _dbContext.Blacklist.FirstAsync(x => x.Channel.BroadcasterTwitchChannelName == Channel);

            await _adminDataService.DeleteBlacklistWordAsync(Channel, word.Id);

            Assert.That(await _dbContext.Blacklist.AnyAsync(x => x.Id == word.Id), Is.False);
        }

        [Test]
        public async Task DeleteBlacklistWord_FromAnotherChannel_IsRejected()
        {
            var word = await _dbContext.Blacklist.FirstAsync(x => x.Channel.BroadcasterTwitchChannelName == Channel);

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _adminDataService.DeleteBlacklistWordAsync(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName, word.Id));

            await Task.CompletedTask;
        }

        // Discord

        [Test]
        public async Task UpdateDiscordConfig_SavesTheIds()
        {
            await _adminDataService.UpdateDiscordConfigAsync(Channel, new UpdateDiscordConfigRequest
            {
                DiscordEnabled = true,
                DiscordGuildId = 123456,
                DiscordEventChannelId = 777
            });

            var config = await _dbContext.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelName == Channel);

            Assert.Multiple(() =>
            {
                Assert.That(config.DiscordEnabled, Is.True);
                Assert.That(config.DiscordGuildId, Is.EqualTo(123456));
                Assert.That(config.DiscordEventChannelId, Is.EqualTo(777));
            });
        }

        [Test]
        public void UpdateDiscordConfig_EnabledWithoutAGuild_IsRejected()
        {
            // there would be nothing for the discord features to act on
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _adminDataService.UpdateDiscordConfigAsync(Channel, new UpdateDiscordConfigRequest
                {
                    DiscordEnabled = true,
                    DiscordGuildId = null
                }));
        }
    }
}
