using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Events
{
    public interface ITwitchEventHandlerService
    {
        Task HandleChannelCheerEvent(BitsCheeredParams cheerParams);
        Task HandleChannelResubscribeEvent(ChannelResubscribeParams resubscribeParams);
        Task HandleChannelGiftSubEvent(ChannelGiftSubParams giftSubParams);
        Task HandleChannelSubEvent(ChannelSubscribeParams subParams);
        Task HandlePredictionEndEvent(ChannelPredictionEndParams channelPredictionEndParams);
        Task HandlePredictionLockedEvent(ChannelPredictionLockedParams channelPredictionLockedParams);
        Task HandlePredictionBeginEvent(ChannelPredictionBeginParams channelPredictionBeginParams);
        Task HandlePollBeginEvent(ChannelPollBeginParams channelPollBeginParams);
        Task HandlePollEndEvent(ChannelPollEndParams channelPollEndParams);
        Task HandleRaidEvent(ChannelRaidParams raidParams);
        Task HandleChannelPointsRedeemedEvent(ChannelPointsRedeemedParams redeemedParams);
        /// <summary>
        /// Handles a stream going live. Pass <paramref name="existingStreamStartedAt"/> when picking up a stream
        /// that was already live when the bot started, so it isn't treated as a brand new stream
        /// </summary>
        Task HandleStreamOnline(string broadcasterId, string broadcasterName, DateTime? existingStreamStartedAt = null);
    }
}
