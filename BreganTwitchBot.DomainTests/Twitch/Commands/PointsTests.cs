using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
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

        _twitchHelperService.Setup(x => x.GetPointsName(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(DatabaseSeedHelper.Channel1ChannelCurrencyName);
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
        var msgParams = MessageParamsHelper.CreateChatMessageParams("!points", "123", new string[] { "!points" }, chatterChannelName: "CoolUser");
        var result = await _pointsDataService.GetPointsAsync(msgParams);

        Assert.That(result, Is.EqualTo("CoolUser has 100 CoolCurrencyName. Rank: 2 / 3"));
    }

    [Test]
    public async Task GetPointsAsync_GetPointsForOtherKnownUser_CorrectDataReturns()
    {
        var msgParams = MessageParamsHelper.CreateChatMessageParams("!points @CoolUser2", "123", new string[] { "!points", "@CoolUser2" });
        var result = await _pointsDataService.GetPointsAsync(msgParams);

        Assert.That(result, Is.EqualTo("CoolUser2 has 73 CoolCurrencyName. Rank: 3 / 3"));
    }

    [Test]
    public async Task GetPointsAsync_GetPointsForUnknownUser_CorrectDataReturns()
    {
        _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>())).ReturnsAsync("999");

        var msgParams = MessageParamsHelper.CreateChatMessageParams("!points @CoolUser3", "123", new string[] { "!points", "@CoolUser3" });
        var result = await _pointsDataService.GetPointsAsync(msgParams);

        Assert.That(result, Is.EqualTo("CoolUser3 has 0 CoolCurrencyName. Rank: N / A"));
    }

    [Test]
    public void AddPointsAsync_ProvideNoCommands_CorrectExceptionMessage()
    {
        var msgParams = MessageParamsHelper.CreateChatMessageParams("!addpoints", "123", new string[] { "!addpoints" }, chatterChannelId: DatabaseSeedHelper.Channel1SuperModUserTwitchUserId, chatterChannelName: DatabaseSeedHelper.Channel1SuperModUserTwitchUsername);
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(msgParams);
        });

        Assert.That(ex.Message, Is.EqualTo("The format is !addpoints <username> <points>"));
    }

    [Test]
    public void AddPointsAsync_ProvideNoPoints_CorrectExceptionMessage()
    {
        var msgParams = MessageParamsHelper.CreateChatMessageParams("!addpoints CoolUser1", "123", new string[] { "!addpoints", "CoolUser1" }, chatterChannelId: DatabaseSeedHelper.Channel1SuperModUserTwitchUserId, chatterChannelName: DatabaseSeedHelper.Channel1SuperModUserTwitchUsername);
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(msgParams);
        });

        Assert.That(ex.Message, Is.EqualTo("The format is !addpoints <username> <points>"));
    }

    [Test]
    public void AddPointsAsync_ProvideUserWithoutSuperMod_CorrectExceptionMessage()
    {
        var msgParams = MessageParamsHelper.CreateChatMessageParams("!addpoints CoolUser1 100", "123", new string[] { "!addpoints", "CoolUser1", "100" });
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(msgParams);
        });

        Assert.That(ex.Message, Is.EqualTo("You don't have permission to do that"));
    }

    [Test]
    [TestCase("tilly")]
    [TestCase("999999999999999999999999999999999999999999999999999999999999999999999999999")]
    [TestCase("-300")]
    public void AddPointAsync_ProvideInvalidPointsValue_CorrectExceptionMessage(string pointsValue)
    {
        var msgParams = MessageParamsHelper.CreateChatMessageParams($"!addpoints CoolUser1 {pointsValue}", "123", new string[] { "!addpoints", "CoolUser1", pointsValue }, chatterChannelId: DatabaseSeedHelper.Channel1SuperModUserTwitchUserId, chatterChannelName: DatabaseSeedHelper.Channel1SuperModUserTwitchUsername);
        var ex = Assert.ThrowsAsync<InvalidCommandException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(msgParams);
        });

        Assert.That(ex.Message, Is.EqualTo("The points to add must be a number greater than 0"));
    }

    [Test]
    public void AddPointsAsync_ProvideInvalidUsername_CorrectExceptionMessage()
    {
        var msgParams = MessageParamsHelper.CreateChatMessageParams("!addpoints CoolUser3 100", "123", new string[] { "!addpoints", "CoolUser3", "100" }, chatterChannelId: DatabaseSeedHelper.Channel1SuperModUserTwitchUserId, chatterChannelName: DatabaseSeedHelper.Channel1SuperModUserTwitchUsername);
        var ex = Assert.ThrowsAsync<TwitchUserNotFoundException>(async () =>
        {
            await _pointsDataService.AddPointsAsync(msgParams);
        });

        Assert.That(ex.Message, Is.EqualTo("User CoolUser3 not found"));
    }

    [Test]
    public async Task AddPointsAsync_ProvideValidUsernameAndPoints_CorrectAmountOfPointsAdd()
    {
        var msgParams = MessageParamsHelper.CreateChatMessageParams("!addpoints CoolUser 100", "123", new string[] { "!addpoints", "CoolUser", "100" }, chatterChannelId: DatabaseSeedHelper.Channel1SuperModUserTwitchUserId, chatterChannelName: DatabaseSeedHelper.Channel1SuperModUserTwitchUsername);
        await _pointsDataService.AddPointsAsync(msgParams);

        var userPoints = await _dbContext.ChannelUserData.FirstAsync(x => x.ChannelUser.TwitchUsername == "cooluser");

        // this is only for testing bc executeupdateasync doesnt update the entity in memory
        await _dbContext.Entry(userPoints).ReloadAsync();

        Assert.That(userPoints.Points, Is.EqualTo(200));
    }
}