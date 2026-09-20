using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Uptime
{
    public class UptimeCommandService(IUptimeDataService uptimeDataService, ITwitchHelperService twitchHelperService)
    {
        /// <summary>
        /// Last time the command ran, per channel. The old bot kept this in a single static
        /// field, which would have let one busy channel silence the command everywhere.
        ///
        /// This is static because the command service is scoped - a new instance exists per
        /// command, so an instance field would reset the cooldown every time.
        /// </summary>
        private static readonly ConcurrentDictionary<string, DateTime> UptimeCooldowns = new();

        private static readonly TimeSpan CooldownPeriod = TimeSpan.FromSeconds(5);

        [TwitchCommand("uptime")]
        public async Task UptimeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            // supermods bypass the cooldown
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            if (!isSuperMod && IsOnCooldown(msgParams.BroadcasterChannelId))
            {
                return;
            }

            UptimeCooldowns[msgParams.BroadcasterChannelId] = DateTime.UtcNow;

            var response = await uptimeDataService.GetStreamUptime(msgParams);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
        }

        [TwitchCommand("botuptime")]
        public async Task BotUptimeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var response = uptimeDataService.GetBotUptime(msgParams);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
        }

        private static bool IsOnCooldown(string broadcasterChannelId)
        {
            return UptimeCooldowns.TryGetValue(broadcasterChannelId, out var lastUsed) && DateTime.UtcNow - lastUsed <= CooldownPeriod;
        }
    }
}
