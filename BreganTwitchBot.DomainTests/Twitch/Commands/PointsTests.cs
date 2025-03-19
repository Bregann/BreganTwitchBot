using Moq;
using Microsoft.EntityFrameworkCore;
using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.DomainTests.Helpers;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;

namespace BreganTwitchBot.DomainTests;

public class PointsTests
{
    private AppDbContext _dbContext;
    private DbConnection _connection;

    private PointsDataService _pointsDataService;
    private Mock<ITwitchHelperService> _twitchHelperService;

    [SetUp]
    public async Task Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        await DatabaseSeedHelper.SeedDatabase(_dbContext);

        _twitchHelperService = new Mock<ITwitchHelperService>();
        _pointsDataService = new PointsDataService(_dbContext, _twitchHelperService.Object);

        _twitchHelperService.Setup(x => x.IsUserSuperModInChannel("123", "1111")).ReturnsAsync(true);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task GetPointsAsync_GetPointsForKnownUser_CorrectDataReturns()
    {
        var result = await _pointsDataService.GetPointsAsync(new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelName = "CoolStreamerName",
            IsBroadcaster = false,
            IsMod = false,
            IsSub = false,
            IsVip = false,
            Message = "!points",
            MessageId = "123",
            BroadcasterChannelId = "123",
            ChatterChannelId = "456",
            ChatterChannelName = "CoolUser",
            MessageParts = new string[] { "!points" }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TwitchUsername, Is.EqualTo("CoolUser"));
            Assert.That(result.Points, Is.EqualTo(100));
            Assert.That(result.Position, Is.EqualTo("1 / 2"));
        });
    }

    [Test]
    public async Task GetPointsAsync_GetPointsForOtherKnownUser_CorrectDataReturns()
    {
        _twitchHelperService.Setup(x => x.GetTwitchUserIdFromUsername(It.IsAny<string>())).ReturnsAsync("789");

        var result = await _pointsDataService.GetPointsAsync(new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelName = "CoolStreamerName",
            IsBroadcaster = false,
            IsMod = false,
            IsSub = false,
            IsVip = false,
            Message = "!points @CoolUser2",
            MessageId = "123",
            BroadcasterChannelId = "123",
            ChatterChannelId = "456",
            ChatterChannelName = "CoolUser",
            MessageParts = new string[] { "!points", "@CoolUser2" }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TwitchUsername, Is.EqualTo("CoolUser2"));
            Assert.That(result.Points, Is.EqualTo(73));
            Assert.That(result.Position, Is.EqualTo("2 / 2"));
        });
    }

    [Test]
    public async Task GetPointsAsync_GetPointsForUnknownUser_CorrectDataReturns()
    {
        var result = await _pointsDataService.GetPointsAsync(new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelName = "CoolStreamerName",
            IsBroadcaster = false,
            IsMod = false,
            IsSub = false,
            IsVip = false,
            Message = "!points @CoolUser3",
            MessageId = "123",
            BroadcasterChannelId = "123",
            ChatterChannelId = "456",
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
                BroadcasterChannelName = "CoolStreamerName",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints",
                MessageId = "123",
                BroadcasterChannelId = "123",
                ChatterChannelId = "1111",
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
                BroadcasterChannelName = "CoolStreamerName",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints CoolUser1",
                MessageId = "123",
                BroadcasterChannelId = "123",
                ChatterChannelId = "1111",
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
                BroadcasterChannelName = "CoolStreamerName",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints CoolUser1 100",
                MessageId = "123",
                BroadcasterChannelId = "123",
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
                BroadcasterChannelName = "CoolStreamerName",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = $"!addpoints CoolUser1 {pointsValue}",
                MessageId = "123",
                BroadcasterChannelId = "123",
                ChatterChannelId = "1111",
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
                BroadcasterChannelName = "CoolStreamerName",
                IsBroadcaster = false,
                IsMod = false,
                IsSub = false,
                IsVip = false,
                Message = "!addpoints CoolUser1 100",
                MessageId = "123",
                BroadcasterChannelId = "123",
                ChatterChannelId = "1111",
                ChatterChannelName = "SuperModUser",
                MessageParts = new string[] { "!addpoints", "CoolUser1", "100" }
            });
        });

        Assert.That(ex.Message, Is.EqualTo("User CoolUser3 not found"));
    }
}
