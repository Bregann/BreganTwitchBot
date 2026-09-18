using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.StreamInfo
{
    public class ChannelInfoDataService(ITwitchApiConnection twitchApiConnection, ITwitchApiInteractionService twitchApiInteractionService, ITwitchHelperService twitchHelperService) : IChannelInfoDataService
    {
        public async Task<string> GetTitleAsync(ChannelChatMessageReceivedParams msgParams)
        {
            var channelInfo = await TryGetChannelInformation(msgParams);

            if (channelInfo == null)
            {
                return $"@{msgParams.ChatterChannelName} => hooo I could not get the channel title sorry try again later";
            }

            return $"@{msgParams.ChatterChannelName} => The current stream title is {channelInfo.Value.Info.Title}";
        }

        public async Task<string> SetTitleAsync(ChannelChatMessageReceivedParams msgParams, string newTitle)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            var channelInfo = await TryGetChannelInformation(msgParams);

            if (channelInfo == null)
            {
                return $"@{msgParams.ChatterChannelName} => hooo I could not get the channel title sorry try again later";
            }

            try
            {
                // the game has to be sent back unchanged or twitch clears it
                await twitchApiInteractionService.ModifyChannelInformationAsync(channelInfo.Value.ApiClient, msgParams.BroadcasterChannelId, newTitle, channelInfo.Value.Info.GameId);
                return $"@{msgParams.ChatterChannelName} => The stream title has been updated to {newTitle} and the current game is {channelInfo.Value.Info.GameName} :)";
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Channel Info] Error updating the title for {msgParams.BroadcasterChannelName}");
                return $"@{msgParams.ChatterChannelName} => well that broke (couldn't update the title)";
            }
        }

        public async Task<string> GetGameAsync(ChannelChatMessageReceivedParams msgParams)
        {
            var channelInfo = await TryGetChannelInformation(msgParams);

            if (channelInfo == null)
            {
                return $"@{msgParams.ChatterChannelName} => hooo I could not get the game sorry try again later";
            }

            return $"@{msgParams.ChatterChannelName} => The current game is {channelInfo.Value.Info.GameName}";
        }

        public async Task<string> SetGameAsync(ChannelChatMessageReceivedParams msgParams, string newGame)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            var channelInfo = await TryGetChannelInformation(msgParams);

            if (channelInfo == null)
            {
                return $"@{msgParams.ChatterChannelName} => hooo I could not get the channel sorry try again later";
            }

            try
            {
                var game = await twitchApiInteractionService.GetGameByNameAsync(channelInfo.Value.ApiClient, newGame);

                if (game == null)
                {
                    return $"@{msgParams.ChatterChannelName} => I could not find a game called {newGame} on Twitch";
                }

                // the title has to be sent back unchanged or twitch clears it
                await twitchApiInteractionService.ModifyChannelInformationAsync(channelInfo.Value.ApiClient, msgParams.BroadcasterChannelId, channelInfo.Value.Info.Title, game.Value.Id);
                return $"@{msgParams.ChatterChannelName} => The game has been updated to {game.Value.Name} and the current title is {channelInfo.Value.Info.Title} :)";
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Channel Info] Error updating the game for {msgParams.BroadcasterChannelName}");
                return $"@{msgParams.ChatterChannelName} => well that broke (couldn't update the game)";
            }
        }

        /// <summary>
        /// Gets the channel's current title/game along with the broadcaster api client used to
        /// fetch it, as updating the channel needs that same broadcaster token.
        /// </summary>
        private async Task<(TwitchLib.Api.TwitchAPI ApiClient, DTOs.Twitch.Api.GetChannelInformationResponse Info)?> TryGetChannelInformation(ChannelChatMessageReceivedParams msgParams)
        {
            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(msgParams.BroadcasterChannelName);

            if (apiClient == null)
            {
                Log.Error($"[Channel Info] No broadcaster api client for {msgParams.BroadcasterChannelName}");
                return null;
            }

            try
            {
                var info = await twitchApiInteractionService.GetChannelInformationAsync(apiClient.ApiClient, msgParams.BroadcasterChannelId);

                if (info == null)
                {
                    Log.Warning($"[Channel Info] No channel information returned for {msgParams.BroadcasterChannelName}");
                    return null;
                }

                return (apiClient.ApiClient, info);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Channel Info] Error getting the channel information for {msgParams.BroadcasterChannelName}");
                return null;
            }
        }
    }
}
