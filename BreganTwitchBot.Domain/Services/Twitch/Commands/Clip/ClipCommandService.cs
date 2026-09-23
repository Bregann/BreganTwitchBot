using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Clip
{
    public class ClipCommandService(IClipDataService clipDataService, ITwitchHelperService twitchHelperService)
    {
        /// <summary>
        /// Last time a clip was made, per channel. Static because the command service is scoped,
        /// so an instance field would reset the cooldown every time.
        /// </summary>
        private static readonly ConcurrentDictionary<string, DateTime> ClipCooldowns = new();

        /// <summary>
        /// A clip covers the last 30 seconds, so a second clip inside that window would mostly
        /// be the same footage. It also keeps chat from spamming twitch's clip endpoint.
        /// </summary>
        private static readonly TimeSpan CooldownPeriod = TimeSpan.FromSeconds(30);

        [TwitchCommand("clip")]
        public async Task ClipCommand(ChannelChatMessageReceivedParams msgParams)
        {
            // supermods bypass the cooldown
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            if (!isSuperMod && IsOnCooldown(msgParams.BroadcasterChannelId))
            {
                return;
            }

            ClipCooldowns[msgParams.BroadcasterChannelId] = DateTime.UtcNow;

            var response = await clipDataService.CreateClip(msgParams);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
        }

        private static bool IsOnCooldown(string broadcasterChannelId)
        {
            return ClipCooldowns.TryGetValue(broadcasterChannelId, out var lastUsed) && DateTime.UtcNow - lastUsed <= CooldownPeriod;
        }
    }
}
