using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Serilog;
using System.Diagnostics;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Uptime
{
    public class UptimeDataService(ITwitchApiConnection twitchApiConnection, ITwitchApiInteractionService twitchApiInteractionService) : IUptimeDataService
    {
        public async Task<string> GetStreamUptimeAsync(ChannelChatMessageReceivedParams msgParams)
        {
            var apiClient = twitchApiConnection.GetBroadcasterApiClientFromChannelName(msgParams.BroadcasterChannelName);

            if (apiClient == null)
            {
                Log.Error($"[Stream Uptime] No broadcaster api client for {msgParams.BroadcasterChannelName}");
                return "Task failed succesfully KEKW (Error getting uptime)";
            }

            try
            {
                var stream = await twitchApiInteractionService.GetStreams(apiClient.ApiClient, msgParams.BroadcasterChannelId);

                if (stream == null)
                {
                    return $"{msgParams.BroadcasterChannelName} is not live :(";
                }

                var uptime = DateTime.UtcNow - stream.StartedAt;
                return $"@{msgParams.ChatterChannelName} => Sadly {msgParams.BroadcasterChannelName} has been streaming for {DurationFormatHelper.Humanise(uptime)}";
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[Stream Uptime] Error getting stream uptime for {msgParams.BroadcasterChannelName}");
                return "Task failed succesfully KEKW (Error getting uptime)";
            }
        }

        public string GetBotUptime(ChannelChatMessageReceivedParams msgParams)
        {
            var botUptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return $"@{msgParams.ChatterChannelName} => The bot has been up for {DurationFormatHelper.Humanise(botUptime)}";
        }
    }
}
