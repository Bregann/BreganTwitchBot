using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Uptime
{
    public class UptimeCommandService(IServiceProvider serviceProvider)
    {
        /// <summary>
        /// Last time the uptime command ran, per channel. The old bot kept this in a single
        /// static field, which would have let one busy channel silence the command everywhere.
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _uptimeCooldowns = new();

        private static readonly TimeSpan CooldownPeriod = TimeSpan.FromSeconds(5);

        [TwitchCommand("uptime")]
        public async Task UptimeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var uptimeDataService = scope.ServiceProvider.GetRequiredService<IUptimeDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                // supermods bypass the cooldown
                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                if (!isSuperMod && IsOnCooldown(msgParams.BroadcasterChannelId))
                {
                    return;
                }

                _uptimeCooldowns[msgParams.BroadcasterChannelId] = DateTime.UtcNow;

                var response = await uptimeDataService.GetStreamUptimeAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
        }

        [TwitchCommand("botuptime")]
        public async Task BotUptimeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var uptimeDataService = scope.ServiceProvider.GetRequiredService<IUptimeDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var response = uptimeDataService.GetBotUptime(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
        }

        private bool IsOnCooldown(string broadcasterChannelId)
        {
            return _uptimeCooldowns.TryGetValue(broadcasterChannelId, out var lastUsed) && DateTime.UtcNow - lastUsed <= CooldownPeriod;
        }
    }
}
