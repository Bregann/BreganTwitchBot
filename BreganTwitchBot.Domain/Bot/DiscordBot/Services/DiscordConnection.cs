using BreganTwitchBot.Core;
using BreganTwitchBot.Core.DiscordBot.Services;
using BreganTwitchBot.DiscordBot.Events;
using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Domain;
using BreganUtils.ProjectMonitor.Projects;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Data;
using System.Reflection;

namespace BreganTwitchBot.Services
{
    public class DiscordConnection
    {
        public static DiscordSocketClient DiscordClient;
        private static InteractionService _interactionService;
        private static IServiceProvider _services;
        private static bool botLoaded = false;
        public static bool ConnectionStatus;

        public static async Task MainAsync()
        {
            DiscordClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 2000,
                AlwaysDownloadUsers = true,
                LogGatewayIntentWarnings = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            DiscordClient.Ready += Ready;
            DiscordClient.Disconnected += Disconnected;
            DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;
            DiscordClient.InteractionCreated += InteractionCreated;
            DiscordClient.Log += LogData;
            await DiscordClient.LoginAsync(TokenType.Bot, AppConfig.DiscordAPIKey);
            await DiscordClient.StartAsync();
            _services = new ServiceCollection().AddSingleton(DiscordClient).AddSingleton<InteractionService>().BuildServiceProvider();
        }

        private static Task Disconnected(Exception arg)
        {
            ConnectionStatus = false;
            Log.Warning("[Discord] Disconnected from Discord");
            BackgroundJob.Schedule(() => HangfireJobs.SendDiscordConnectionStatusToProjectMonitor(), TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private static Task LogData(LogMessage arg)
        {
            Log.Information($"{arg}");
            return Task.CompletedTask;
        }

        private static async Task InteractionCreated(SocketInteraction arg)
        {
            var isMod = DiscordHelper.IsUserMod(arg.User.Id);

            var command = arg as SocketSlashCommand;

            if (command == null)
            {
                return;
            }

            if (arg.Channel.Id != AppConfig.DiscordSocksChannelID && command.CommandName == "socks") //socks channel   713365310408884236
            {
                await arg.RespondAsync("Please use the socks maker channel!", ephemeral: true);
                return;
            }

            if (command.CommandName == "socks" && arg.Channel.Id == AppConfig.DiscordSocksChannelID)
            {
                var contextSocks = new SocketInteractionContext(DiscordClient, arg);
                await _interactionService.ExecuteCommandAsync(contextSocks, _services);
                return;
            }

            if (command.CommandName == "link" && arg.Channel.Id == AppConfig.DiscordLinkingChannelID)
            {
                var contextLinking = new SocketInteractionContext(DiscordClient, arg);
                await _interactionService.ExecuteCommandAsync(contextLinking, _services);
                return;
            }

            if (arg.Channel.Id != AppConfig.DiscordCommandsChannelID && !isMod)
            {
                await arg.RespondAsync("Please use the bot commands channel!", ephemeral: true);
            }

            var context = new SocketInteractionContext(DiscordClient, arg);
            await _interactionService.ExecuteCommandAsync(context, _services);
        }

        private static async Task GuildMembersDownloaded(SocketGuild arg)
        {
            Log.Information("[Discord] Members downloaded");
        }

        private static async Task Ready()
        {
            ConnectionStatus = true;

            if (botLoaded)
            {
                Log.Information("[Discord Connection] Discord client already setup");
                return;
            }

            await DiscordEvents.SetupDiscordEvents();
            CustomCommands.SetupCustomCommands();

            await DiscordClient.SetGameAsync("many users", type: ActivityType.Watching);
            await DiscordClient.DownloadUsersAsync(DiscordClient.Guilds);

            _interactionService = new InteractionService(DiscordClient.Rest);
            await _interactionService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), null);
#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(AppConfig.DiscordGuildID, true);
#else
            await _interactionService.RegisterCommandsGloballyAsync();

#endif
            botLoaded = true;
            Log.Information("[Discord Connection] Discord Client Ready");
        }
    }
}