using Blazor.Analytics;
using BreganTwitchBot.Web.Data.Commands;
using BreganTwitchBot.Web.Data.Leaderboards;
using BreganTwitchBot.Web.Data.Stats;
using BreganTwitchBot.Web.Data.UserSearch;
using MudBlazor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Async(x => x.File("Logs/log.log", retainedFileCountLimit: null, rollingInterval: RollingInterval.Day)).WriteTo.Console().CreateLogger();
Log.Information("Logger Setup");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<TopStatsService>();
builder.Services.AddSingleton<GamblingStatsService>();
builder.Services.AddSingleton<CustomCommandsService>();
builder.Services.AddSingleton<UserSearchService>();
builder.Services.AddSingleton<LeaderboardsService>();
builder.Services.AddMudServices();
builder.Services.AddGoogleAnalytics("G-8S83WNS8CG");
builder.Logging.AddSerilog();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();