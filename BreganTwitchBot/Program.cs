using BreganTwitchBot.Domain.Data.Services;
using BreganTwitchBot.Domain.Data.Services.Discord;
using BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.BookRecs;
using BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Daily;
using BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Gambling;
using BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.GeneralCommands;
using BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Levelling;
using BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Linking;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands._8Ball;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.CustomCommands;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.DadJoke;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.DailyPoints;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.FollowAge;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Gambling;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Hours;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Leaderboards;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Linking;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.TwitchBosses;
using BreganTwitchBot.Domain.Data.Services.Twitch.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Data.Services.Twitch.Events;
using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Helpers;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch.Events;
using Discord.Interactions;
using Discord.WebSocket;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TwitchLib.EventSub.Websockets.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(x => x.File("/app/Logs/log.log", retainedFileCountLimit: null, rollingInterval: RollingInterval.Day))
    .WriteTo.Console()
    .Enrich.WithProperty("Application", "BreganTwitchBot-Api" + (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "-Test" : ""))
    .WriteTo.Seq("http://192.168.1.20:5341")
    .CreateLogger();

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
           .UseNpgsql(Environment.GetEnvironmentVariable("BTBConnStringLive")));
builder.Services.AddScoped<AppDbContext>(provider => provider.GetService<PostgresqlContext>());

GlobalConfiguration.Configuration.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(Environment.GetEnvironmentVariable("BTBConnStringLive")));

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(Environment.GetEnvironmentVariable("BTBConnStringLive")))
        );
#endif

// The twitch service
builder.Services.AddSingleton<ITwitchApiConnection, TwitchApiConnection>();

// Helper bits
builder.Services.AddSingleton<IConfigHelperService, ConfigHelperService>();

// Twitch events
builder.Services.AddTwitchLibEventSubWebsockets();
builder.Services.AddHostedService<WebsocketHostedService>();
builder.Services.AddSingleton<ITwitchHelperService, TwitchHelperService>();
builder.Services.AddSingleton<ITwitchApiInteractionService, TwitchApiInteractionService>();
builder.Services.AddSingleton<IConfigHelperService, ConfigHelperService>();

builder.Services.AddSingleton<ITwitchEventHandlerService, TwitchEventHandlerService>();

// Twitch commands
builder.Services.AddSingleton<ICommandHandler, CommandHandler>();

builder.Services.AddSingleton<PointsCommandService>();
builder.Services.AddScoped<IPointsDataService, PointsDataService>();

builder.Services.AddSingleton<FollowAgeCommandService>();
builder.Services.AddScoped<IFollowAgeDataService, FollowAgeDataService>();

builder.Services.AddSingleton<EightBallCommandService>();
builder.Services.AddSingleton<DadJokesCommandService>();

builder.Services.AddSingleton<CustomCommandsCommandService>();
builder.Services.AddScoped<ICustomCommandDataService, CustomCommandsDataService>();

builder.Services.AddSingleton<DailyPointsCommandService>();
builder.Services.AddScoped<IDailyPointsDataService, DailyPointsDataService>();

builder.Services.AddSingleton<GamblingCommandService>();
builder.Services.AddScoped<IGamblingDataService, GamblingDataService>();

builder.Services.AddSingleton<HoursCommandService>();
builder.Services.AddScoped<IHoursDataService, HoursDataService>();

builder.Services.AddSingleton<LinkingCommandService>();
builder.Services.AddScoped<ILinkingDataService, LinkingDataService>();

builder.Services.AddSingleton<LeaderboardsCommandService>();
builder.Services.AddScoped<ILeaderboardsDataService, LeaderboardsDataService>();

builder.Services.AddSingleton<TwitchBossesCommandService>();
builder.Services.AddSingleton<ITwitchBossesDataService, TwitchBossesDataService>();

builder.Services.AddSingleton<WordBlacklistCommandService>();
builder.Services.AddScoped<IWordBlacklistDataService, WordBlacklistDataService>();
builder.Services.AddSingleton<IWordBlacklistMonitorService, WordBlacklistMonitorService>();

// Discord

// Add the new registration with a factory function:
builder.Services.AddSingleton(serviceProvider => // 'sp' or 'serviceProvider' is the DI container itself
{
    var clientProvider = serviceProvider.GetRequiredService<IDiscordClientProvider>();
    var discordClient = clientProvider.Client;
    return new InteractionService(discordClient);
});

// These lines remain the same
builder.Services.AddSingleton<IDiscordService, DiscordService>();
builder.Services.AddSingleton(sp => (IDiscordClientProvider)sp.GetRequiredService<IDiscordService>());

builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<IDiscordHelperService, DiscordHelperService>();
builder.Services.AddSingleton<IDiscordUserLookupService, DiscordUserLookupService>();
builder.Services.AddScoped<IDiscordEventHelperService, DiscordEventHelperService>();
builder.Services.AddScoped<IDiscordRoleManagerService, DiscordRoleManagerService>();

builder.Services.AddScoped<IDiscordDailyPointsData, DiscordDailyPointsData>();
builder.Services.AddScoped<IDiscordGamblingData, DiscordGamblingData>();
builder.Services.AddScoped<IGeneralCommandsData, GeneralCommandsData>();
builder.Services.AddScoped<IDiscordLevellingData, DiscordLevellingData>();
builder.Services.AddScoped<IDiscordLinkingData, DiscordLinkingData>();
builder.Services.AddScoped<IDiscordBookRecsData, DiscordBookRecsData>();

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

// start everything
var twitchApi = app.Services.GetRequiredService<ITwitchApiConnection>();
await twitchApi.InitialiseConnectionsAsync();

var discordService = app.Services.GetService<IDiscordService>()!;
await discordService.StartAsync();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
    hangfireJobs.SetupHangfireJobs();
}

app.Run();