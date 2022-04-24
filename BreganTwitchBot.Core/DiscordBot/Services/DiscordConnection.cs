using BreganTwitchBot.Core.DiscordBot.Services;
using BreganTwitchBot.DiscordBot.Events;
using BreganTwitchBot.DiscordBot.Helpers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace BreganTwitchBot.Services
{
    public class DiscordConnection
    {
        public static DiscordSocketClient DiscordClient;
        private static InteractionService _interactionService;
        private static IServiceProvider _services;

        public static async Task MainAsync()
        {
            DiscordClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 2000
            });

            DiscordClient.Ready += Ready;
            DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;
            DiscordClient.InteractionCreated += InteractionCreated;
            await DiscordClient.LoginAsync(TokenType.Bot, Config.DiscordAPIKey);
            await DiscordClient.StartAsync();
            _services = new ServiceCollection().AddSingleton(DiscordClient).AddSingleton<InteractionService>().BuildServiceProvider();
        }

        private static async Task InteractionCreated(SocketInteraction arg)
        {
            var isMod = DiscordHelper.IsUserMod(arg.User.Id);

            var command = arg as SocketSlashCommand;

            if (command == null)
            {
                return;
            }

            if (arg.Channel.Id != 713365310408884236 && command.CommandName == "socks") //socks channel   713365310408884236
            {
                await arg.RespondAsync("Please use the socks maker channel!", ephemeral: true);
                return;
            }

            if (command.CommandName == "link" && arg.Channel.Id == Config.DiscordLinkingChannelID)
            {
                var contextLinking = new SocketInteractionContext(DiscordClient, arg);
                await _interactionService.ExecuteCommandAsync(contextLinking, _services);
                return;
            }

            if (arg.Channel.Id != Config.DiscordCommandsChannelID && !isMod)
            {
                await arg.RespondAsync("Please use the bot commands channel!", ephemeral: true);
            }

            var context = new SocketInteractionContext(DiscordClient, arg);
            await _interactionService.ExecuteCommandAsync(context, _services);
        }

        private static Task GuildMembersDownloaded(SocketGuild arg)
        {
            Log.Information("[Discord] Members downloaded");
            return Task.CompletedTask;
        }

        private static async Task Ready()
        {
            await DiscordEvents.SetupDiscordEvents();
            CustomCommands.SetupCustomCommands();

            await DiscordClient.SetGameAsync("test", type: ActivityType.Watching);
            await DiscordClient.DownloadUsersAsync(DiscordClient.Guilds);

            _interactionService = new InteractionService(DiscordClient.Rest);
            await _interactionService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), null);
#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(Config.DiscordGuildID, true);
#else
            await _interactionService.RegisterCommandsGloballyAsync();

#endif
            Log.Information("[Discord Connection] Discord Client Ready");
        }
    }
}
