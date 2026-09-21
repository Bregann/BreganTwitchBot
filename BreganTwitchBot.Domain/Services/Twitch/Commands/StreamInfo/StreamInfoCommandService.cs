using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.StreamInfo
{
    public class StreamInfoCommandService(IStreamInfoDataService streamInfoDataService, ITwitchHelperService twitchHelperService)
    {
        // static because the command service is scoped - a new instance exists per command,
        // so an instance field would reset the cooldown every time
        private static readonly CommandCooldownHelper FollowersCooldown = new(TimeSpan.FromSeconds(5));
        private static readonly CommandCooldownHelper SubsCooldown = new(TimeSpan.FromSeconds(5));

        [TwitchCommand("followers", ["followercount", "followcount"])]
        public async Task FollowersCommand(ChannelChatMessageReceivedParams msgParams)
        {
            // supermods bypass the cooldown
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            if (!isSuperMod && FollowersCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
            {
                return;
            }

            FollowersCooldown.StartCooldown(msgParams.BroadcasterChannelId);

            var response = await streamInfoDataService.GetFollowerCount(msgParams);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
        }

        [TwitchCommand("subs", ["subcount", "subscribers"])]
        public async Task SubsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            if (!isSuperMod && SubsCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
            {
                return;
            }

            SubsCooldown.StartCooldown(msgParams.BroadcasterChannelId);

            var response = await streamInfoDataService.GetSubscriberCount(msgParams);
            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
        }
    }
}
