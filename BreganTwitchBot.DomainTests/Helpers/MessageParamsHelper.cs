using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.DomainTests.Helpers;

public class MessageParamsHelper
{
    public static ChannelChatMessageReceivedParams CreateChatMessageParams(
        string message,
        string messageId,
        string[] messageParts,
        string broadcasterChannelId = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId,
        string broadcasterChannelName = DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName,
        string chatterChannelId = DatabaseSeedHelper.Channel1User1TwitchUserId,
        string chatterChannelName = DatabaseSeedHelper.Channel1User1TwitchUsername,
        bool isBroadcaster = false,
        bool isMod = false,
        bool isSub = false,
        bool isVip = false)
    {
        return new ChannelChatMessageReceivedParams
        {
            BroadcasterChannelId = broadcasterChannelId,
            BroadcasterChannelName = broadcasterChannelName,
            ChatterChannelId = chatterChannelId,
            ChatterChannelName = chatterChannelName,
            IsBroadcaster = isBroadcaster,
            IsMod = isMod,
            IsSub = isSub,
            IsVip = isVip,
            Message = message,
            MessageId = messageId,
            MessageParts = messageParts
        };
    }
}
