using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ISubathonDataService
    {
        /// <summary>
        /// Adds time for bits cheered, at the rate for the subathon's current total
        /// </summary>
        Task AddBitsTime(string broadcasterChannelId, string chatterChannelId, int bitsAmount);

        /// <summary>
        /// Adds time for a sub or gifted subs, at the rate for the subathon's current total
        /// </summary>
        Task AddSubTime(string broadcasterChannelId, string chatterChannelId, SubTierEnum subTier, int subCount = 1);

        /// <summary>
        /// Adds time manually. Moderator only.
        /// </summary>
        Task<string> AddTimeManually(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// The current subathon status message for chat
        /// </summary>
        Task<string> GetSubathonStatus(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Starts a subathon in the channel. Moderator only.
        /// </summary>
        Task<string> StartSubathon(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Stops the running subathon. Super mods and the broadcaster only.
        /// </summary>
        Task<string> StopSubathon(ChannelChatMessageReceivedParams msgParams);
    }
}
