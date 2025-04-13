using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordService : IDiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly IEnvironmentalSettingHelper _environmentalSettingHelper;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _services;

        public DiscordSocketClient Client => _client;

        public DiscordService(IEnvironmentalSettingHelper environmentalSettingHelper, IServiceProvider serviceProvider)
        {
            _environmentalSettingHelper = environmentalSettingHelper;
            _services = serviceProvider;

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 2000,
                AlwaysDownloadUsers = true,
                LogGatewayIntentWarnings = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            _interactionService = new InteractionService(_client.Rest);
        }

        public async Task StartAsync()
        {
            var token = _environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.DiscordBotToken);

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Discord bot token is not set.");
            }

            // Hook the handler BEFORE starting
#if DEBUG
            _client.Ready += async () =>
            {
                await _interactionService.RegisterCommandsToGuildAsync(196696160486948864, true);
            };
#else
            _client.Ready += async () =>
            {
                await _interactionService.RegisterCommandsGloballyAsync(true);
            };
#endif

            _client.InteractionCreated += async interaction =>
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(ctx, _services);
            };

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Log.Information("Discord Setup");
        }

        public async Task StopAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}
