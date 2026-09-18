using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface ISubathonDataService
    {
        /// <summary>
        /// Adds time for bits cheered, at the rate for the subathon's current total
        /// </summary>
        Task AddBitsTimeAsync(string broadcasterChannelId, string chatterChannelId, int bitsAmount);

        /// <summary>
        /// Adds time for a sub or gifted subs, at the rate for the subathon's current total
        /// </summary>
        Task AddSubTimeAsync(string broadcasterChannelId, string chatterChannelId, SubTierEnum subTier, int subCount = 1);

        /// <summary>
        /// Adds time manually. Moderator only.
        /// </summary>
        Task<string> AddTimeManuallyAsync(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// The current subathon status message for chat
        /// </summary>
        Task<string> GetSubathonStatusAsync(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Starts a subathon in the channel. Moderator only.
        /// </summary>
        Task<string> StartSubathonAsync(ChannelChatMessageReceivedParams msgParams);

        /// <summary>
        /// Stops the running subathon. Moderator only.
        /// </summary>
        Task<string> StopSubathonAsync(ChannelChatMessageReceivedParams msgParams);
    }
}
