using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands._8Ball;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.CustomCommands;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.DadJoke;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Helpers;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TwitchLib.EventSub.Websockets.Extensions;

Log.Logger = new LoggerConfiguration().WriteTo.Async(x => x.File("/app/Logs/log.log", retainedFileCountLimit: null, rollingInterval: RollingInterval.Day)).WriteTo.Console().CreateLogger();
Log.Information("Logger Setup");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

// Add in our own services
builder.Services.AddSingleton<IEnvironmentalSettingHelper, EnvironmentalSettingHelper>();

// Add in the auth
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JwtValidIssuer"),
            ValidAudience = Environment.GetEnvironmentVariable("JwtValidAudience"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtKey")!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

#if DEBUG
builder.Services.AddDbContext<PostgresqlContext>(options =>
    options.UseLazyLoadingProxies()
           .UseNpgsql(Environment.GetEnvironmentVariable("BTBConnStringTest")));

builder.Services.AddScoped<AppDbContext>(provider => provider.GetService<PostgresqlContext>());

GlobalConfiguration.Configuration.UseMemoryStorage();

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage()
        );

#else
builder.Services.AddDbContext<PostgresqlContext>(options =>
    options.UseLazyLoadingProxies()
           .UseNpgsql(Environment.GetEnvironmentVariable("xxxConnStringLive")));
builder.Services.AddScoped<AppDbContext>(provider => provider.GetService<PostgresqlContext>());

GlobalConfiguration.Configuration.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(Environment.GetEnvironmentVariable("xxxConnString")));

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(Environment.GetEnvironmentVariable("xxxConnString")))
        );
#endif

// The twitch service
builder.Services.AddSingleton<ITwitchApiConnection>(provider =>
{
    using (var scope = provider.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var channelsToConnectTo = dbContext.Channels.ToArray();

        var connection = new TwitchApiConnection();

        foreach (var channel in channelsToConnectTo)
        {
            connection.Connect(channel.BroadcasterTwitchChannelName, channel.Id, channel.BroadcasterTwitchChannelId, channel.BroadcasterTwitchChannelOAuthToken, channel.BroadcasterTwitchChannelRefreshToken, AccountType.Broadcaster, channel.BroadcasterTwitchChannelId, channel.BroadcasterTwitchChannelName);
            connection.Connect(channel.BotTwitchChannelName, channel.Id, channel.BotTwitchChannelId, channel.BotTwitchChannelOAuthToken, channel.BotTwitchChannelRefreshToken, AccountType.Bot, channel.BroadcasterTwitchChannelId, channel.BroadcasterTwitchChannelName);
        }
        return connection;
    }
});

// Twitch events
builder.Services.AddTwitchLibEventSubWebsockets();
builder.Services.AddHostedService<WebsocketHostedService>();
builder.Services.AddSingleton<ITwitchHelperService, TwitchHelperService>();
builder.Services.AddSingleton<ITwitchApiInteractionService, TwitchApiInteractionService>();

// Twitch commands
builder.Services.AddSingleton<CommandHandler>();

builder.Services.AddSingleton<PointsCommandService>();
builder.Services.AddScoped<IPointsDataService, PointsDataService>();

builder.Services.AddSingleton<FollowAgeCommandService>();
builder.Services.AddScoped<IFollowAgeDataService, FollowAgeDataService>();

builder.Services.AddSingleton<EightBallCommandService>();
builder.Services.AddSingleton<DadJokesCommandService>();

builder.Services.AddSingleton<CustomCommandsCommandService>();
builder.Services.AddScoped<ICustomCommandDataService, CustomCommandsDataService>();

// hangfire
builder.Services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(10));

builder.Services.AddScoped<HangfireJobServiceHelper>();

var app = builder.Build();

app.UseCors("AllowAll");

#if DEBUG
// Seed the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetService<AppDbContext>()!;
    var settingsHelper = scope.ServiceProvider.GetRequiredService<IEnvironmentalSettingHelper>();

    if (dbContext.Database.GetPendingMigrations().Any())
    {
        await DatabaseSeedHelper.SeedDatabase(dbContext, settingsHelper, scope.ServiceProvider);
        await dbContext.Database.MigrateAsync();
    }
}
#endif

var environmentalSettingHelper = app.Services.GetService<IEnvironmentalSettingHelper>()!;
await environmentalSettingHelper.LoadEnvironmentalSettings();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard();

var auth = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
{
    RequireSsl = false,
    SslRedirect = false,
    LoginCaseSensitive = true,
    Users = new []
    {
        new BasicAuthAuthorizationUser
        {
            Login = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.HangfireUsername),
            PasswordClear = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.HangfirePassword)
        }
    }
})};

app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = auth
}, JobStorage.Current);

using (var scope = app.Services.CreateScope())
{
    var hangfireJobs = scope.ServiceProvider.GetRequiredService<HangfireJobServiceHelper>();
    await hangfireJobs.SetupHangfireJobs();
}


app.Run();