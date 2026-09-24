using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Clip
{
    public class ClipDataService(ITwitchApiConnection twitchApiConnection, ITwitchApiInteractionService twitchApiInteractionService) : IClipDataService
    {
        public async Task<string> CreateClip(ChannelChatMessageReceivedParams msgParams)
        {
            // the bot account has the clips:edit scope, so every clip is made by the bot rather
            // than needing each broadcaster to grant it
            var apiClient = twitchApiConnection.GetBotApiClient();

            if (apiClient == null)
            {
                Log.Error("[Clip] No bot api client");
                return "Task failed succesfully KEKW (Error creating clip)";
            }

            try
            {
                var stream = await twitchApiInteractionService.GetStreams(apiClient.ApiClient, msgParams.BroadcasterChannelId);

                if (stream == null)
                {
                    return $"@{msgParams.ChatterChannelName} => {msgParams.BroadcasterChannelName} is not live so there is nothing to clip :(";
                }

                var clipId = await twitchApiInteractionService.CreateClip(apiClient.ApiClient, msgParams.BroadcasterChannelId);

                if (clipId == null)
                {
                    return "Task failed succesfully KEKW (Error creating clip)";
                }

                Log.Information($"[Clip] {msgParams.ChatterChannelName} clipped {msgParams.BroadcasterChannelName} - {clipId}");
                return $"@{msgParams.ChatterChannelName} => Clipped! https://clips.twitch.tv/{clipId}";
            }
            catch (Exception ex)
            {
                // twitch refuses clips on some channels (e.g. clips disabled or the bot restricted)
                Log.Warning(ex, $"[Clip] Error creating clip for {msgParams.BroadcasterChannelName}");
                return "Task failed succesfully KEKW (Error creating clip)";
            }
        }
    }
}
