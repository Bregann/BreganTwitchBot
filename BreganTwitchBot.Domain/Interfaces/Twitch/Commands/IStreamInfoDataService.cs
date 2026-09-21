using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IStreamInfoDataService
    {
        /// <summary>
        /// Gets the channel's total follower count
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> GetFollowerCount(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Gets the channel's total subscriber count. Needs the broadcaster's own token.
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> GetSubscriberCount(ChannelChatMessageReceivedParams msgParams);
    }
}
