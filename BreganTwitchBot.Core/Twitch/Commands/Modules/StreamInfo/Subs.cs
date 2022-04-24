using BreganTwitchBot.Services;
using BreganTwitchBot.Twitch.Helpers;
using Serilog;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.StreamInfo
{
    public class Subs
    {
        private static DateTime _subsCooldown;

        public static async Task HandleSubsCommand(string username)
        {
            if (DateTime.Now - TimeSpan.FromSeconds(5) <= _subsCooldown)
            {
                Log.Information("[Twitch Commands] !subs command handled successfully (cooldown)");
                return;
            }

            int subCount;
            try
            {
                var subReq = await TwitchApiConnection.ApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(Config.TwitchChannelID, accessToken: Config.BroadcasterOAuth);
                subCount = subReq.Total;
            }
            catch (Exception e)
            {
                TwitchHelper.SendMessage($"@{username} => There has been an error getting the sub count - Please try again :)");
                _subsCooldown = DateTime.Now;
                Log.Warning($"[Twitch Command] !subs command errored - {e}");
                return;
            }

            TwitchHelper.SendMessage($"@{username} => {Config.BroadcasterName} has {subCount} subs");
            _subsCooldown = DateTime.Now;
            return;
        }
    }
}
