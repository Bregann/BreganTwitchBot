namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordMessageModerationService
    {
        /// <summary>
        /// Checks a Discord message against the channel's blacklist and the scam link
        /// heuristic, muting the author and alerting the mods if it matches.
        /// </summary>
        /// <returns>True when the message was acted on, so the caller can skip awarding xp</returns>
        Task<bool> CheckMessage(ulong guildId, ulong channelId, ulong userId, string messageContent, bool authorIsBot);
    }
}
