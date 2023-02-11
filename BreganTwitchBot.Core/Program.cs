using BreganTwitchBot;
using BreganTwitchBot.Domain;
using BreganUtils.ProjectMonitor;
using Hangfire;
using Hangfire.Dashboard.Dark;
using Hangfire.PostgreSql;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Async(x => x.File("Data/Logs/log.log", retainedFileCountLimit: null, rollingInterval: RollingInterval.Day)).WriteTo.Console().CreateLogger(); 
Log.Information("Logger Setup");
AppConfig.LoadConfig(); 

//Setup project monitor
#if DEBUG
ProjectMonitorConfig.SetupMonitor("debug", AppConfig.ProjectMonitorApiKey);
#else
ProjectMonitorConfig.SetupMonitor("release", AppConfig.ProjectMonitorApiKey);
#endif

ProjectMonitorCommon.ReportProjectUp("twitchbot");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

#if RELEASE
builder.WebHost.UseUrls("http://localhost:5005");
#endif

// Add services to the container.
JobStorage.Current = new PostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "twitchbot" });

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "twitchbot" })
        .UseDarkDashboard()
        );

builder.Services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(10));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await SetupBot.Setup();

app.Run();
