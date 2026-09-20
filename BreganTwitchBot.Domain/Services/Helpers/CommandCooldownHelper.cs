using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services.Helpers
{
    /// <summary>
    /// Per channel command cooldowns.
    ///
    /// The old bot kept a cooldown in a single static field per command, which was fine when it
    /// only ever served one channel. With a single bot across many channels that would let one
    /// busy channel silence a command everywhere, so cooldowns are tracked per channel here.
    /// </summary>
    public class CommandCooldownHelper(TimeSpan cooldownPeriod)
    {
        private readonly ConcurrentDictionary<string, DateTime> _lastUsed = new();

        /// <summary>
        /// Returns true if the command is still cooling down in this channel
        /// </summary>
        public bool IsOnCooldown(string broadcasterChannelId)
        {
            return _lastUsed.TryGetValue(broadcasterChannelId, out var lastUsed) && DateTime.UtcNow - lastUsed <= cooldownPeriod;
        }

        /// <summary>
        /// Marks the command as just used in this channel, starting the cooldown
        /// </summary>
        public void StartCooldown(string broadcasterChannelId)
        {
            _lastUsed[broadcasterChannelId] = DateTime.UtcNow;
        }
    }
}
