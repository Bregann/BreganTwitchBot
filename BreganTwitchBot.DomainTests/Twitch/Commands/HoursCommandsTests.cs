using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Services.Twitch;
using BreganTwitchBot.Domain.Services.Twitch.Commands.Hours;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;
using TwitchLib.Api;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    public class HoursCommandsTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<ITwitchApiInteractionService> _twitchApiInteractionService;
        private Mock<ITwitchApiConnection> _twitchApiConnection;
        private Mock<ITwitchHelperService> _twitchHelperService;
        private Mock<IDiscordRoleManagerService> _discordRoleManagerService;
        private Mock<IConfigHelperService> _configHelperService;

        private HoursDataService _hoursDataService;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .WithCleanUp(true) // Cleanup after test run
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

            _twitchApiInteractionService = new Mock<ITwitchApiInteractionService>();
            _twitchApiConnection = new Mock<ITwitchApiConnection>();
            _twitchHelperService = new Mock<ITwitchHelperService>();
            _discordRoleManagerService = new Mock<IDiscordRoleManagerService>();
            _configHelperService = new Mock<IConfigHelperService>();

            // The single bot account is shared across every channel
            _twitchApiConnection.Setup(x => x.GetBotApiClient())
                .Returns(new TwitchApiConnection.TwitchAccount(new TwitchAPI(), "", "", "", "", Domain.Enums.AccountType.Bot));

            // Mock channel 2 broadcaster to be null
            _twitchApiConnection.Setup(x => x.GetBroadcasterApiClientFromChannelName(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName))
                .Returns(value: null);

            _hoursDataService = new HoursDataService(_dbContext, _twitchApiInteractionService.Object, _twitchApiConnection.Object, _twitchHelperService.Object, _discordRoleManagerService.Object, _configHelperService.Object);
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
        public async Task UpdateWatchtimeForChannel_BroadcasterNotLive_WatchtimeNotUpdated()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = false;
            await _dbContext.SaveChangesAsync();

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            var userWatchtime = await _dbContext.ChannelUserWatchtime.FirstAsync(x => x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);

            Assert.Multiple(() =>
            {
                Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(0));
                Assert.That(userWatchtime.MinutesWatchedThisWeek, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesWatchedThisMonth, Is.EqualTo(2));
                Assert.That(userWatchtime.MinutesWatchedThisYear, Is.EqualTo(3));
                Assert.That(userWatchtime.MinutesInStream, Is.EqualTo(4));
            });
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_ApiClientNull_DoesNotUpdateWatchtime()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            // the single bot account is shared by every channel, so if it isn't connected then
            // no channel can have its watchtime updated
            _twitchApiConnection.Setup(x => x.GetBotApiClient()).Returns(value: null);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId);

            var userWatchtime = await _dbContext.ChannelUserWatchtime
                .FirstOrDefaultAsync(x => x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId && x.ChannelId == channel.Id);

            if (userWatchtime != null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(0));
                    Assert.That(userWatchtime.MinutesWatchedThisWeek, Is.EqualTo(1));
                    Assert.That(userWatchtime.MinutesWatchedThisMonth, Is.EqualTo(2));
                    Assert.That(userWatchtime.MinutesWatchedThisYear, Is.EqualTo(3));
                    Assert.That(userWatchtime.MinutesInStream, Is.EqualTo(4));
                });
            }

            _twitchApiInteractionService.Verify(x => x.GetChatters(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_NewUser_AddsUserAndWatchtime()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            var newUser = new Chatters { UserId = "newuser123", UserName = "NewUser" };
            var chattersResult = new GetChattersResponse { Chatters = new List<Chatters> { newUser } };

            _twitchApiInteractionService.Setup(x => x.GetChatters(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chattersResult);

            _twitchHelperService.Setup(x => x.AddOrUpdateUserToDatabase(channel.BroadcasterTwitchChannelId, newUser.UserId, channel.BroadcasterTwitchChannelName, newUser.UserName, false, false))
                .Returns(Task.CompletedTask);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            _twitchHelperService.Verify(x => x.AddOrUpdateUserToDatabase(
                channel.BroadcasterTwitchChannelId,
                newUser.UserId,
                channel.BroadcasterTwitchChannelName,
                newUser.UserName,
                false,
                false), Times.Once);
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_ExistingUser_WatchtimeUpdates()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            var newUser = new Chatters { UserId = DatabaseSeedHelper.Channel1User1TwitchUserId, UserName = DatabaseSeedHelper.Channel1User1TwitchUsername };
            var chattersResult = new GetChattersResponse { Chatters = new List<Chatters> { newUser } };

            _twitchApiInteractionService.Setup(x => x.GetChatters(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chattersResult);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var userWatchtime = await _dbContext.ChannelUserWatchtime.FirstAsync(x => x.ChannelUser.TwitchUsername == DatabaseSeedHelper.Channel1User1TwitchUsername);

            Assert.Multiple(() =>
            {
                Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(1));
                Assert.That(userWatchtime.MinutesWatchedThisWeek, Is.EqualTo(2));
                Assert.That(userWatchtime.MinutesWatchedThisMonth, Is.EqualTo(3));
                Assert.That(userWatchtime.MinutesWatchedThisYear, Is.EqualTo(4));
                Assert.That(userWatchtime.MinutesInStream, Is.EqualTo(5));
            });
        }

        [Test]
        public async Task UpdateWatchtimeForChannel_UserReachesRankup_RankUpAddsToDatabase()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;
            await _dbContext.SaveChangesAsync();

            var newUser = new Chatters { UserId = DatabaseSeedHelper.Channel1User2TwitchUserId, UserName = DatabaseSeedHelper.Channel1User2TwitchUsername };
            var chattersResult = new GetChattersResponse { Chatters = new List<Chatters> { newUser } };

            _twitchApiInteractionService.Setup(x => x.GetChatters(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(chattersResult);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var userRanks = await _dbContext.ChannelUserRankProgress.FirstAsync(x => x.ChannelUser.TwitchUsername == DatabaseSeedHelper.Channel1User2TwitchUsername);

            Assert.That(userRanks, Is.Not.Null);
        }

        [Test]
        [TestCase(HoursWatchTypes.Stream, "this stream!")]
        [TestCase(HoursWatchTypes.Week, "this week!")]
        [TestCase(HoursWatchTypes.Month, "this month!")]
        [TestCase(HoursWatchTypes.AllTime, "in the stream!")]
        public async Task GetHoursCommand_ValidUser_ReturnsCorrectMessage(HoursWatchTypes hoursType, string expectedMessageType)
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours" });
            await _dbContext.SaveChangesAsync();

            var response = await _hoursDataService.GetHoursCommand(msgParams, hoursType);

            Assert.That(response, Does.Contain(expectedMessageType));
            Assert.That(response, Does.Contain("about"));
        }

        [Test]
        public void GetHoursCommand_InvalidUser_ThrowsTwitchUserNotFoundException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours invalid", "123", new string[] { "!hours", "invalid" });

            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var ex = Assert.ThrowsAsync<TwitchUserNotFoundException>(() => _hoursDataService.GetHoursCommand(msgParams, HoursWatchTypes.AllTime));
            Assert.That(ex.Message, Is.EqualTo("User not found!"));
        }

        [Test]
        public void GetHoursCommand_UserWithNoWatchtime_ThrowsTwitchUserNotFoundException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours" }, chatterChannelId: "invalid", chatterChannelName: "invalid");

            var ex = Assert.ThrowsAsync<TwitchUserNotFoundException>(() => _hoursDataService.GetHoursCommand(msgParams, HoursWatchTypes.AllTime));
            Assert.That(ex.Message, Is.EqualTo("Oh dear this user doesn't have any watchtime in the channel!"));
        }

        [Test]
        public void GetHoursCommand_InvalidHoursType_ThrowsArgumentOutOfRangeException()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours" });

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _hoursDataService.GetHoursCommand(msgParams, (HoursWatchTypes)999));
        }

        [Test]
        [TestCase(HoursWatchTypes.Stream, "this stream!")]
        [TestCase(HoursWatchTypes.Week, "this week!")]
        [TestCase(HoursWatchTypes.Month, "this month!")]
        [TestCase(HoursWatchTypes.AllTime, "in the stream!")]
        public async Task GetHoursCommand_ValidOtherUser_ReturnsCorrectMessage(HoursWatchTypes hoursType, string expectedMessageType)
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!hours", "123", new string[] { "!hours", "cooluser2" });
            await _dbContext.SaveChangesAsync();

            _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>()))
                .ReturnsAsync("789");

            var response = await _hoursDataService.GetHoursCommand(msgParams, hoursType);

            Assert.That(response, Does.Contain(expectedMessageType));
            Assert.That(response, Does.Contain("cooluser2 has"));
        }

        [Test]
        public async Task ResetStreamMinutesForBroadcaster_ValidBroadcaster_ResetsMinutes()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            await _hoursDataService.ResetStreamMinutesForBroadcaster(channel.BroadcasterTwitchChannelId);

            var userWatchtime = await _dbContext.ChannelUserWatchtime.FirstAsync(x => x.ChannelUser.TwitchUserId == DatabaseSeedHelper.Channel1User1TwitchUserId);
            Assert.That(userWatchtime.MinutesWatchedThisStream, Is.EqualTo(0));
        }

        // grouped rank up messages

        /// <summary>
        /// Adds users one minute off the seeded 10 minute "tilly" rank, and makes the stream live
        /// with enough chat since the last rank up message
        /// </summary>
        private async Task<List<Chatters>> UsersAboutToRankUp(int count, int minutesInStream = 9, ulong discordUserId = 0)
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            channel.ChannelConfig.BroadcasterLive = true;

            var chatters = new List<Chatters>();

            for (var i = 0; i < count; i++)
            {
                var user = new ChannelUser
                {
                    AddedOn = DateTime.UtcNow,
                    CanUseOpenAi = false,
                    DiscordUserId = discordUserId == 0 ? 0 : discordUserId + (ulong)i,
                    TwitchUserId = $"rankup{minutesInStream}-{i}",
                    TwitchUsername = $"rankuser{minutesInStream}x{i}",
                    LastSeen = DateTime.UtcNow
                };

                await _dbContext.ChannelUsers.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                await _dbContext.ChannelUserData.AddAsync(new ChannelUserData { ChannelUserId = user.Id, ChannelId = channel.Id, Points = 0, InStream = true, IsSub = false, IsVip = false, IsSuperMod = false, TimeoutStrikes = 0, WarnStrikes = 0 });
                await _dbContext.ChannelUserWatchtime.AddAsync(new ChannelUserWatchtime { ChannelUserId = user.Id, ChannelId = channel.Id, MinutesInStream = minutesInStream, MinutesWatchedThisStream = 0, MinutesWatchedThisWeek = 0, MinutesWatchedThisMonth = 0, MinutesWatchedThisYear = 0 });

                chatters.Add(new Chatters { UserId = user.TwitchUserId, UserName = user.TwitchUsername });
            }

            await _dbContext.SaveChangesAsync();

            _twitchHelperService.Setup(x => x.GetChatMessageCount(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId)).Returns(10);
            _twitchHelperService.Setup(x => x.HasUserChattedInCurrentStream(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            return chatters;
        }

        private void ChattersAre(List<Chatters> chatters)
        {
            _twitchApiInteractionService.Setup(x => x.GetChatters(It.IsAny<TwitchAPI>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new GetChattersResponse { Chatters = chatters });
        }

        private List<string> RankUpMessagesSent()
        {
            return _twitchHelperService.Invocations
                .Where(x => x.Method.Name == nameof(ITwitchHelperService.SendTwitchMessageToChannel))
                .Select(x => (string)x.Arguments[2])
                .Where(x => x.StartsWith("Congrats"))
                .ToList();
        }

        [Test]
        public async Task RankUps_ThreeAtOnce_AreOneMessage()
        {
            var users = await UsersAboutToRankUp(3);
            ChattersAre(users);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var messages = RankUpMessagesSent();
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages[0], Does.StartWith($"Congrats @{users[0].UserName}, @{users[1].UserName} and @{users[2].UserName}, you earned the tilly rank"));
            _twitchHelperService.Verify(x => x.ResetChatMessageCount(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId), Times.Once());
        }

        [Test]
        public async Task RankUps_MoreThanThree_NamesThreeAndCountsTheRest()
        {
            var users = await UsersAboutToRankUp(7);
            ChattersAre(users);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var messages = RankUpMessagesSent();
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(messages[0], Does.Contain("4 other people also earned it!"));
                Assert.That(messages[0].Count(c => c == '@'), Is.EqualTo(3));
            });

            // everyone still got the rank
            var ranked = await _dbContext.ChannelUserRankProgress.CountAsync(x => x.ChannelUser.TwitchUserId.StartsWith("rankup"));
            Assert.That(ranked, Is.EqualTo(7));
        }

        [Test]
        public async Task RankUps_PeopleWhoHaventChatted_AreCountedButNotPinged()
        {
            var users = await UsersAboutToRankUp(4);
            ChattersAre(users);

            _twitchHelperService.Setup(x => x.HasUserChattedInCurrentStream(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _twitchHelperService.Setup(x => x.HasUserChattedInCurrentStream(It.IsAny<string>(), users[1].UserId)).Returns(true);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var messages = RankUpMessagesSent();
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages[0], Does.StartWith($"Congrats @{users[1].UserName}, you earned"));
            Assert.That(messages[0], Does.Contain("3 other people also earned it!"));
        }

        [Test]
        public async Task RankUps_NobodyHasChatted_NoMessage()
        {
            var users = await UsersAboutToRankUp(3);
            ChattersAre(users);
            _twitchHelperService.Setup(x => x.HasUserChattedInCurrentStream(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(RankUpMessagesSent(), Is.Empty);
            _twitchHelperService.Verify(x => x.ResetChatMessageCount(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task RankUps_QuietChat_NoMessage()
        {
            var users = await UsersAboutToRankUp(3);
            ChattersAre(users);
            _twitchHelperService.Setup(x => x.GetChatMessageCount(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId)).Returns(HoursDataService.ChatMessagesBetweenRankUpMessages - 1);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(RankUpMessagesSent(), Is.Empty);
        }

        [Test]
        public async Task RankUps_DifferentRanks_OneMessageEach()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            await _dbContext.ChannelRanks.AddAsync(new ChannelRank { ChannelId = channel.Id, RankName = "gold", RankMinutesRequired = 20, BonusRankPointsEarned = 100 });
            await _dbContext.SaveChangesAsync();

            var tilly = await UsersAboutToRankUp(2, minutesInStream: 9);
            var gold = await UsersAboutToRankUp(2, minutesInStream: 19);
            ChattersAre([.. tilly, .. gold]);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            var messages = RankUpMessagesSent();
            Assert.That(messages, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(messages[0], Does.Contain("the tilly rank").And.Contain(tilly[0].UserName).And.Contain(tilly[1].UserName));
                Assert.That(messages[1], Does.Contain("the gold rank").And.Contain(gold[0].UserName).And.Contain(gold[1].UserName));
            });
        }

        [Test]
        public async Task RankUps_MoreRanksThanTheLimit_OnlySendsTheLimit()
        {
            var channel = await _dbContext.Channels.FirstAsync(c => c.BroadcasterTwitchChannelId == DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);
            await _dbContext.ChannelRanks.AddAsync(new ChannelRank { ChannelId = channel.Id, RankName = "gold", RankMinutesRequired = 20, BonusRankPointsEarned = 100 });
            await _dbContext.ChannelRanks.AddAsync(new ChannelRank { ChannelId = channel.Id, RankName = "diamond", RankMinutesRequired = 30, BonusRankPointsEarned = 100 });
            await _dbContext.SaveChangesAsync();

            ChattersAre([.. await UsersAboutToRankUp(1, 9), .. await UsersAboutToRankUp(1, 19), .. await UsersAboutToRankUp(1, 29)]);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(RankUpMessagesSent(), Has.Count.EqualTo(HoursDataService.MaxRankUpMessagesPerMinute));
        }

        [Test]
        public async Task RankUps_EveryLinkedUserGetsTheirRole_EvenWhenNotNamed()
        {
            _configHelperService.Setup(x => x.IsDiscordEnabled(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId)).Returns(true);

            var users = await UsersAboutToRankUp(5, discordUserId: 5000);
            ChattersAre(users);

            // no message goes out at all, which used to mean nobody got their role
            _twitchHelperService.Setup(x => x.GetChatMessageCount(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId)).Returns(0);

            // the role is worked out from the saved ranks, so the new one has to be saved first
            var rankSavedWhenApplied = new List<bool>();
            _discordRoleManagerService.Setup(x => x.ApplyRoleOnDiscordWatchtimeRankup(It.IsAny<string>(), DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId))
                .Callback<string, string>((twitchUserId, _) =>
                {
                    using var checkContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_postgresContainer.GetConnectionString()).Options);
                    rankSavedWhenApplied.Add(checkContext.ChannelUserRankProgress.Any(x => x.ChannelUser.TwitchUserId == twitchUserId));
                })
                .Returns(Task.CompletedTask);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(RankUpMessagesSent(), Is.Empty);
            Assert.That(rankSavedWhenApplied, Has.Count.EqualTo(5).And.All.True);
        }

        [Test]
        public async Task RankUps_ADiscordError_DoesNotLoseTheRank()
        {
            _configHelperService.Setup(x => x.IsDiscordEnabled(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId)).Returns(true);
            _discordRoleManagerService.Setup(x => x.ApplyRoleOnDiscordWatchtimeRankup(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("discord is down"));

            var users = await UsersAboutToRankUp(2, discordUserId: 6000);
            ChattersAre(users);

            await _hoursDataService.UpdateWatchtimeForChannel(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.Multiple(async () =>
            {
                Assert.That(await _dbContext.ChannelUserRankProgress.CountAsync(x => x.ChannelUser.TwitchUserId.StartsWith("rankup")), Is.EqualTo(2));
                Assert.That(RankUpMessagesSent(), Has.Count.EqualTo(1));
            });
        }
    }
}
