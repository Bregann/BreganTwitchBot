using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.StreamInfo
{
    public class StreamInfoCommandService(IServiceProvider serviceProvider)
    {
        private readonly CommandCooldownHelper _followersCooldown = new(TimeSpan.FromSeconds(5));
        private readonly CommandCooldownHelper _subsCooldown = new(TimeSpan.FromSeconds(5));

        [TwitchCommand("followers", ["followercount", "followcount"])]
        public async Task FollowersCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var streamInfoDataService = scope.ServiceProvider.GetRequiredService<IStreamInfoDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                // supermods bypass the cooldown
                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                if (!isSuperMod && _followersCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
                {
                    return;
                }

                _followersCooldown.StartCooldown(msgParams.BroadcasterChannelId);

                var response = await streamInfoDataService.GetFollowerCountAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
        }

        [TwitchCommand("subs", ["subcount", "subscribers"])]
        public async Task SubsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var streamInfoDataService = scope.ServiceProvider.GetRequiredService<IStreamInfoDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                if (!isSuperMod && _subsCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
                {
                    return;
                }

                _subsCooldown.StartCooldown(msgParams.BroadcasterChannelId);

                var response = await streamInfoDataService.GetSubscriberCountAsync(msgParams);
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
        }
    }
}
