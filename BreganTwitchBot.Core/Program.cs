using BreganTwitchBot;
using BreganTwitchBot.Core.Twitch.Commands.Modules.WordBlacklist;
using BreganTwitchBot.Events;
using BreganTwitchBot.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration().WriteTo.File("Logs/log.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: null).WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information).CreateLogger();
Log.Information("Logger Setup");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

#region Config
await Config.LoadConfigAndInsertDatabaseFieldsIfNeeded();
await JobScheduler.SetupJobScheduler();
#endregion

#region Twitch
var bot = new TwitchBotConnection();
var twitchThread = new Thread(bot.Connect().GetAwaiter().GetResult);
twitchThread.Start();
Log.Information("[Twitch Client] Started Twitch Thread");

var twitchApi = new TwitchApiConnection();
twitchApi.Connect();
Log.Information("[Twitch API] Connected To Twitch API");

var pubSub = new TwitchPubSubConnection();
pubSub.Connect();
Log.Information("[Twitch PubSub] Connected To Twitch PubSub");

TwitchEvents.SetupTwitchEvents();
WordBlacklist.LoadBlacklistedWords();
#endregion

#region Discord
//Start Discord
var discordThread = new Thread(DiscordConnection.MainAsync().GetAwaiter().GetResult);
discordThread.Start();
#endregion

await Task.Delay(-1);