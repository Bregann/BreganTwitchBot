using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IChannelInfoDataService
    {
        /// <summary>
        /// Gets the channel's current stream title
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> GetTitle(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Sets the channel's stream title. Moderator only.
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> SetTitle(ChannelChatMessageReceivedParams msgParams, string newTitle);

        /// <summary>
        /// Gets the channel's current game
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> GetGame(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Sets the channel's game. Moderator only.
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> SetGame(ChannelChatMessageReceivedParams msgParams, string newGame);
    }
}
