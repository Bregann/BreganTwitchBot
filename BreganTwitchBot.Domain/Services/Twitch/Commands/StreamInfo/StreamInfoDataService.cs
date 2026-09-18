using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.StreamInfo
{
    public class StreamInfoDataService(ITwitchApiConnection twitchApiConnection, ITwitchApiInteractionService twitchApiInteractionService) : IStreamInfoDataService
    {
        public async Task<string> GetFollowerCount(ChannelChatMessageReceivedParams msgParams)
        {
            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(msgParams.BroadcasterChannelName);

            if (apiClient == null)
            {
                Log.Error($"[Stream Info] No broadcaster api client for {msgParams.BroadcasterChannelName}");
                return $"@{msgParams.ChatterChannelName} => oh no something broke what a shame 4Head";
            }

            try
            {
                var followerCount = await twitchApiInteractionService.GetChannelFollowerCountAsync(apiClient.ApiClient, msgParams.BroadcasterChannelId, apiClient.TwitchChannelClientId);
                return $"@{msgParams.ChatterChannelName} => {msgParams.BroadcasterChannelName} has {followerCount:N0} followers";
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[Stream Info] !followers command errored for {msgParams.BroadcasterChannelName}");
                return $"@{msgParams.ChatterChannelName} => oh no something broke what a shame 4Head";
            }
        }

        public async Task<string> GetSubscriberCount(ChannelChatMessageReceivedParams msgParams)
        {
            // the sub count is only readable with the broadcaster's own token
            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(msgParams.BroadcasterChannelName);

            if (apiClient == null)
            {
                Log.Error($"[Stream Info] No broadcaster api client for {msgParams.BroadcasterChannelName}");
                return $"@{msgParams.ChatterChannelName} => There has been an error getting the sub count - Please try again :)";
            }

            try
            {
                var subCount = await twitchApiInteractionService.GetChannelSubscriberCountAsync(apiClient.ApiClient, msgParams.BroadcasterChannelId);
                return $"@{msgParams.ChatterChannelName} => {msgParams.BroadcasterChannelName} has {subCount:N0} subs";
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Stream Info] !subs command errored for {msgParams.BroadcasterChannelName}");
                return $"@{msgParams.ChatterChannelName} => There has been an error getting the sub count - Please try again :)";
            }
        }
    }
}
