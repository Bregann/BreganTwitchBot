using Moq;
using Microsoft.EntityFrameworkCore;
using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.DomainTests.Helpers;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Twitch.Commands;

public class PointsTests
{
    private PostgreSqlContainer _postgresContainer;
    private AppDbContext _dbContext;
    private Mock<ITwitchHelperService> _twitchHelperService;
    private PointsDataService _pointsDataService;

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

        _twitchHelperService = new Mock<ITwitchHelperService>();
        _pointsDataService = new PointsDataService(_dbContext, _twitchHelperService.Object);

        _twitchHelperService.Setup(x => x.IsUserSuperModInChannel("123", "1111")).ReturnsAsync(true);
        _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername("cooluser")).ReturnsAsync("456");
        _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername("cooluser2")).ReturnsAsync("789");
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
    public async Task GetPointsAsync_GetPointsForKnownUser_CorrectDataReturns()
    {
        var result = await _pointsDataService.GetPointsAsync(new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
            IsBroadcaster = false,
            IsMod = false,
            IsSub = false,
            IsVip = false,
            Message = "!points",
            MessageId = "123",
            BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
            ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
            ChatterChannelName = "CoolUser",
            MessageParts = new string[] { "!points" }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TwitchUsername, Is.EqualTo("CoolUser"));
            Assert.That(result.Points, Is.EqualTo(100));
            Assert.That(result.Position, Is.EqualTo("2 / 3"));
        });
    }

    [Test]
    public async Task GetPointsAsync_GetPointsForOtherKnownUser_CorrectDataReturns()
    {
        var result = await _pointsDataService.GetPointsAsync(new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
            IsBroadcaster = false,
            IsMod = false,
            IsSub = false,
            IsVip = false,
            Message = "!points @CoolUser2",
            MessageId = "123",
            BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
            ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
            ChatterChannelName = "CoolUser",
            MessageParts = new string[] { "!points", "@CoolUser2" }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TwitchUsername, Is.EqualTo("CoolUser2"));
            Assert.That(result.Points, Is.EqualTo(73));
            Assert.That(result.Position, Is.EqualTo("3 / 3"));
        });
    }

    [Test]
    public async Task GetPointsAsync_GetPointsForUnknownUser_CorrectDataReturns()
    {
        _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>())).ReturnsAsync("999");

        var result = await _pointsDataService.GetPointsAsync(new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
            IsBroadcaster = false,
            IsMod = false,
            IsSub = false,
            IsVip = false,
            Message = "!points @CoolUser3",
            MessageId = "123",
            BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
            ChatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
            ChatterChannelName = "CoolUser",
            MessageParts = new string[] { "!points", "@CoolUser3" }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TwitchUsername, Is.EqualTo("CoolUser3"));
            Assert.That(result.Points, Is.EqualTo(0));
            Assert.That(result.Position, Is.EqualTo("N/A"));
        });
    }

    [Test]
    public void AddPointsAsync_ProvideNoCommands_CorrectExceptionMessage()
    {
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints",
                MessageId = "123",
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
                ChatterChannelName = "SuperModUser",
                MessageParts = new string[] { "!addpoints" }
            });
        });

        Assert.That(ex.Message, Is.EqualTo("The format is !addpoints <username> <points>"));
    }

    [Test]
    public void AddPointsAsync_ProvideNoPoints_CorrectExceptionMessage()
    {
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints CoolUser1",
                MessageId = "123",
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
                ChatterChannelName = "SuperModUser",
                MessageParts = new string[] { "!addpoints", "CoolUser1" }
            });
        });

        Assert.That(ex.Message, Is.EqualTo("The format is !addpoints <username> <points>"));
    }

    [Test]
    public void AddPointsAsync_ProvideUserWithoutSuperMod_CorrectExceptionMessage()
    {
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints CoolUser1 100",
                MessageId = "123",
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelId = "456",
                ChatterChannelName = "CoolUser",
                MessageParts = new string[] { "!addpoints", "CoolUser1", "100" }
            });
        });

        Assert.That(ex.Message, Is.EqualTo("You don't have permission to do that"));
    }

    [Test]
    [TestCase("tilly")]
    [TestCase("999999999999999999999999999999999999999999999999999999999999999999999999999")]
    [TestCase("-300")]
    public void AddPointAsync_ProvideInvalidPointsValue_CorrectExceptionMessage(string pointsValue)
    {
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = $"!addpoints CoolUser1 {pointsValue}",
                MessageId = "123",
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
                ChatterChannelName = "SuperModUser",
                MessageParts = new string[] { "!addpoints", "CoolUser1", pointsValue }
            });
        });

        Assert.That(ex.Message, Is.EqualTo("The points to add must be a number greater than 0"));
    }

    [Test]
    public void AddPointsAsync_ProvideInvalidUsername_CorrectExceptionMessage()
    {
        var columns = _dbContext.ChannelUserData.ToArray();

        var ex = Assert.ThrowsAsync<TwitchUserNotFoundException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(new ChannelChatMessageReceivedParams
            {
                BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints CoolUser3 100",
                MessageId = "123",
                BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
                ChatterChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
                ChatterChannelName = "SuperModUser",
                MessageParts = new string[] { "!addpoints", "CoolUser3", "100" }
            });
        });

        Assert.That(ex.Message, Is.EqualTo("User CoolUser3 not found"));
    }

    [Test]
    public async Task AddPointsAsync_ProvideValidUsernameAndPoints_CorrectAmountOfPointsAdd()
    {
        await _pointsDataService.AddPointsAsync(new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
            IsBroadcaster = false,
            IsMod = false,
            IsSub = false,
            IsVip = false,
            Message = "!addpoints CoolUser 100",
            MessageId = "123",
            BroadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
            ChatterChannelId = DatabaseSeedHelper.Channel1SuperModUserTwitchUserId,
            ChatterChannelName = "SuperModUser",
            MessageParts = new string[] { "!addpoints", "CoolUser", "100" }
        });

        var userPoints = await _dbContext.ChannelUserData.FirstAsync(x => x.ChannelUser.TwitchUsername == "cooluser");

        // this is only for testing bc executeupdateasync doesnt update the entity in memory
        await _dbContext.Entry(userPoints).ReloadAsync();

        Assert.That(userPoints.Points, Is.EqualTo(200));
    }
}