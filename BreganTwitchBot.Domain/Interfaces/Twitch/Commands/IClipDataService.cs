using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IClipDataService
    {
        /// <summary>
        /// Clips the last few seconds of the broadcaster's stream
        /// </summary>
        /// <returns>The message to send back to chat</returns>
        Task<string> CreateClip(ChannelChatMessageReceivedParams msgParams);
    }
}
