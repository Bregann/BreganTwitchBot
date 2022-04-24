using BreganTwitchBot.Services;
using BreganTwitchBot.Twitch.Helpers;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using System.Diagnostics;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.Uptime
{
    public class Uptime
    {
        private static DateTime _uptimeCooldown;

        public static async Task HandleUptimeCommand(string username, string userId)
        {
            //5 sec cooldown to prevent spam
            if (DateTime.Now - TimeSpan.FromSeconds(5) <= _uptimeCooldown && !SuperMods.SuperMods.IsUserSupermod(userId))
            {
                return;
            }

            //attempt to get the stream time
            TimeSpan? uptime;
            try
            {
                var uptimeReq = await TwitchApiConnection.ApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { Config.TwitchChannelID });

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
                _uptimeCooldown = DateTime.Now;
                return;
            }

            if (uptime == null)
            {
                TwitchHelper.SendMessage($"{Config.BroadcasterName} is not live :(");
                _uptimeCooldown = DateTime.Now;
                return;
            }

            TwitchHelper.SendMessage($"@{username} => Sadly {Config.BroadcasterName} has been streaming for {uptime.Value.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}");
            _uptimeCooldown = DateTime.Now;
        }

        public static void HandleBotUptimeCommand(string username)
        {
            var botUptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            TwitchHelper.SendMessage($"@{username} => The bot has been up for {botUptime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}");
        }
    }
}
