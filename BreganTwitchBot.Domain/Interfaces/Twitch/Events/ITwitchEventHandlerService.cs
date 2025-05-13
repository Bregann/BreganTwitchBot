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
        Task HandleStreamOnline(string broadcasterId, string broadcasterName, bool allowCollectionInstantly = false);
    }
}
