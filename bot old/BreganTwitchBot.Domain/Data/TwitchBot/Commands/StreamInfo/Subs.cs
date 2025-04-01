using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo
{
    public class Subs
    {
        private static DateTime _subsCooldown;

        public static async Task HandleSubsCommand(string username)
        {
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _subsCooldown)
            {
                Log.Information("[Twitch Commands] !subs command handled successfully (cooldown)");
                return;
            }

            try
            {
                var subReq = await TwitchApiConnection.ApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(AppConfig.TwitchChannelID, accessToken: AppConfig.BroadcasterOAuth);
                TwitchHelper.SendMessage($"@{username} => {AppConfig.BroadcasterName} has {subReq.Total} subs");
            }
            catch (Exception e)
            {
                TwitchHelper.SendMessage($"@{username} => There has been an error getting the sub count - Please try again :)");
                _subsCooldown = DateTime.UtcNow;
                Log.Warning($"[Twitch Command] !subs command errored - {e}");
                return;
            }

            _subsCooldown = DateTime.UtcNow;
            return;
        }
    }
}