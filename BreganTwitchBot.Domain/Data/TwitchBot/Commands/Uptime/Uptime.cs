using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using System.Diagnostics;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Uptime
{
    public class Uptime
    {
        private static DateTime _uptimeCooldown;

        public static async Task HandleUptimeCommand(string username, string userId)
        {
            //5 sec cooldown to prevent spam
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _uptimeCooldown && !Supermods.IsUserSupermod(userId))
            {
                return;
            }

            if (AppConfig.StreamerLive == false)
            {
                TwitchHelper.SendMessage($"{AppConfig.BroadcasterName} is not live :(");
                _uptimeCooldown = DateTime.UtcNow;
                return;
            }

            //attempt to get the stream time
            TimeSpan? uptime;
            try
            {
                var uptimeReq = await TwitchApiConnection.ApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { AppConfig.TwitchChannelID });

                if (uptimeReq.Streams.Length == 0)
                {
                    uptime = null;
                }
                else
                {
                    uptime = DateTime.UtcNow - uptimeReq.Streams[0].StartedAt;
                }
            }
            catch (Exception e)
            {
                TwitchHelper.SendMessage("Task failed succesfully KEKW (Error getting uptime)");
                Log.Warning($"[Stream Uptime] Error getting Stream Uptime {e}");
                _uptimeCooldown = DateTime.UtcNow;
                return;
            }

            if (uptime == null)
            {
                TwitchHelper.SendMessage($"{AppConfig.BroadcasterName} is not live :(");
                _uptimeCooldown = DateTime.UtcNow;
                return;
            }

            TwitchHelper.SendMessage($"@{username} => Sadly {AppConfig.BroadcasterName} has been streaming for {uptime.Value.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}");
            _uptimeCooldown = DateTime.UtcNow;
        }

        public static void HandleBotUptimeCommand(string username)
        {
            var botUptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            TwitchHelper.SendMessage($"@{username} => The bot has been up for {botUptime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}");
        }
    }
}