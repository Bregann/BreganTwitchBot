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
        Task HandleStreamOnline(string broadcasterId, string broadcasterName, bool allowCollectionInstantly = false);

        /// <summary>
        /// Called when the bot starts up and finds the stream already live. Only treats it as a new
        /// stream if it isn't the one the bot already knew about before restarting.
        /// </summary>
        Task HandleStreamLiveOnStartup(string broadcasterId, string broadcasterName, DateTime streamStartedAt);
    }
}
