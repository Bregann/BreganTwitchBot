using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IMarblesDataService
    {
        /// <summary>
        /// Adds a marbles win to the user named in the command. Moderator only.
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> AddMarblesWinAsync(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Gets the marbles wins for the user named in the command, or the caller if no user is named
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> GetMarblesWinsAsync(ChannelChatMessageReceivedParams msgParams);
    }
}
