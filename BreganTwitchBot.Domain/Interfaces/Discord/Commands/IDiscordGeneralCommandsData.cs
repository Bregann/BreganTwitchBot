namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordGeneralCommandsData
    {
        /// <summary>
        /// The channel's follower count, for the Discord side
        /// </summary>
        Task<string> GetFollowerCount(ulong guildId);

        /// <summary>
        /// The channel's subscriber count. Needs the broadcaster's own token.
        /// </summary>
        Task<string> GetSubscriberCount(ulong guildId);

        /// <summary>
        /// Adds or removes the channel's configured mute role
        /// </summary>
        /// <returns>The message to reply with</returns>
        Task<string> SetUserMuted(ulong guildId, ulong userId, bool muted);
    }
}
