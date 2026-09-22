using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.RegularExpressions;

namespace BreganTwitchBot.Domain.Services.Discord
{
    /// <summary>
    /// Moderates Discord messages against the channel's word blacklist and a scam link check.
    ///
    /// The blacklist itself is the same per channel one the Twitch side uses, so a word only
    /// has to be banned once. The old bot kept a separate static list loaded at startup, which
    /// meant a word added in chat did not apply in Discord until a restart.
    /// </summary>
    public partial class DiscordMessageModerationService(
        AppDbContext context,
        IDiscordClientProvider discordClientProvider,
        IDiscordHelperService discordHelperService) : IDiscordMessageModerationService
    {
        /// <summary>
        /// Domains a scam link is likely to end in. Discord's own domains are allowed, as a
        /// genuine nitro gift links to them.
        /// </summary>
        private static readonly string[] SuspiciousDomains =
        [
            ".xxx", ".tv", ".travel", ".tel", ".ru", ".org", ".net", ".name", ".museum", ".mobi",
            ".me", ".ly", ".jobs", ".int", ".info", ".gov", ".gg", ".coop", ".xyz", ".com",
            ".co.uk", ".cat", ".biz", "asia", ".aero", ".gift"
        ];

        private static readonly string[] AllowedDomains = ["discord.com", "discord.gg"];

        public async Task<bool> CheckMessage(ulong guildId, ulong channelId, ulong userId, string messageContent, bool authorIsBot)
        {
            if (authorIsBot || string.IsNullOrWhiteSpace(messageContent))
            {
                return false;
            }

            var channel = await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);

            if (channel == null)
            {
                return false;
            }

            var lowered = messageContent.ToLower();

            if (IsLikelyScam(lowered))
            {
                await MuteAndAlert(guildId, channelId, userId, channel.ChannelConfig.DiscordMuteRoleId, "potential scam link");
                return true;
            }

            // strip everything that isn't a letter or digit, so spacing and punctuation can't
            // be used to slip a banned word past the check
            var normalised = NonWordCharacters().Replace(messageContent, "").ToLower();

            var blacklistedWords = await context.Blacklist
                .Where(x => x.ChannelId == channel.Id && x.WordType == WordType.PermBanWord)
                .Select(x => x.Word)
                .ToListAsync();

            if (blacklistedWords.Any(normalised.Contains))
            {
                await MuteAndAlert(guildId, channelId, userId, channel.ChannelConfig.DiscordMuteRoleId, "blacklisted word");
                return true;
            }

            return false;
        }

        /// <summary>
        /// A message mentioning nitro or a gift, carrying a link that isn't Discord's own
        /// </summary>
        private static bool IsLikelyScam(string loweredMessage)
        {
            var mentionsGift = loweredMessage.Contains("nitro") || loweredMessage.Contains(".gift");

            if (!mentionsGift)
            {
                return false;
            }

            // a real gift links to discord itself
            if (AllowedDomains.Any(loweredMessage.Contains))
            {
                return false;
            }

            return SuspiciousDomains.Any(loweredMessage.Contains);
        }

        private async Task MuteAndAlert(ulong guildId, ulong channelId, ulong userId, ulong? muteRoleId, string reason)
        {
            Log.Information($"[Discord Moderation] {reason} from {userId} in {guildId}");

            if (muteRoleId == null)
            {
                Log.Warning($"[Discord Moderation] No mute role configured for guild {guildId}, cannot act on {reason}");
                return;
            }

            var guild = discordClientProvider.Client.GetGuild(guildId);
            var role = guild?.GetRole(muteRoleId.Value);
            var user = guild?.GetUser(userId);

            if (guild == null || role == null || user == null)
            {
                Log.Warning($"[Discord Moderation] Could not mute {userId} in {guildId} - guild, role or user missing");
                return;
            }

            await user.AddRoleAsync(role);
            await discordHelperService.SendMessage(channelId, $"<@{userId}> has been muted - {reason}");
        }

        [GeneratedRegex(@"[^0-9a-zA-Z\p{L}]+")]
        private static partial Regex NonWordCharacters();
    }
}
