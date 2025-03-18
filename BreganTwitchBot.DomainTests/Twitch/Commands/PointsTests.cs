using Moq;
using Microsoft.EntityFrameworkCore;
using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.DomainTests.Helpers;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Enums;
using static BreganTwitchBot.Domain.Data.Services.Twitch.TwitchApiConnection;
using TwitchLib.Api.Interfaces;

namespace BreganTwitchBot.DomainTests;

public class PointsTests
{
    private AppDbContext _dbContext;
    private DbConnection _connection;

    private PointsDataService _pointsDataService;

    [SetUp]
    public async Task Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        await DatabaseSeedHelper.SeedDatabase(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
        _connection.Close();
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}
