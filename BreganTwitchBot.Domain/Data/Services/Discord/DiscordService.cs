using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordService : IDiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly IEnvironmentalSettingHelper _environmentalSettingHelper;
        public DiscordSocketClient Client => _client;

        public DiscordService(IEnvironmentalSettingHelper environmentalSettingHelper)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 2000,
                AlwaysDownloadUsers = true,
                LogGatewayIntentWarnings = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            _environmentalSettingHelper = environmentalSettingHelper;
        }

        public async Task StartAsync()
        {
            var token = _environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.DiscordBotToken);

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Discord bot token is not set.");
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        public async Task StopAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}
