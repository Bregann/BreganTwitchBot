using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IUptimeDataService
    {
        /// <summary>
        /// Gets how long the broadcaster has been live for
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> GetStreamUptime(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Gets how long the bot process has been running for
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        string GetBotUptime(ChannelChatMessageReceivedParams msgParams);
    }
}
