using BreganTwitchBot.Domain.DTOs.Twitch.Api;
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
        /// Twitch says the stream is live on this broadcast. Works out whether that's a new broadcast,
        /// the same one dropping out and coming back, or one the bot already knew about from before
        /// a restart, and only does what's needed for that
        /// </summary>
        Task HandleStreamOnline(string broadcasterId, string broadcasterName, string twitchStreamId, DateTime startedAt);

        Task HandleStreamOffline(string broadcasterId, string broadcasterName);

        /// <summary>
        /// Brings the stored stream status in line with twitch's, for anything the bot missed while
        /// it was down or disconnected. Called on startup and every minute
        /// </summary>
        /// <param name="liveStream">What twitch returned, or null if it isn't live</param>
        Task CheckStreamStatus(string broadcasterId, string broadcasterName, GetStreamsResponse? liveStream);
    }
}
