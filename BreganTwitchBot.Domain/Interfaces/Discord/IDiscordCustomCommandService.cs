namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordCustomCommandService
    {
        /// <summary>
        /// Replies with a channel's custom command if the message is one.
        /// </summary>
        /// <returns>The reply, or null when the message isn't a command this should answer</returns>
        Task<string?> TryHandleCustomCommand(ulong guildId, ulong channelId, string username, string messageContent, bool isMod);
    }
}
