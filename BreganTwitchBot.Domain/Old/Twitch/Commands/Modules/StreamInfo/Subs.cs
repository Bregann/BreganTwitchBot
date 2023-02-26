using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Serilog;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.StreamInfo
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

            int subCount;
            try
            {
                var subReq = await TwitchApiConnection.ApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(AppConfig.TwitchChannelID, accessToken: AppConfig.BroadcasterOAuth);
                subCount = subReq.Total;
            }
            catch (Exception e)
            {
                TwitchHelper.SendMessage($"@{username} => There has been an error getting the sub count - Please try again :)");
                _subsCooldown = DateTime.UtcNow;
                Log.Warning($"[Twitch Command] !subs command errored - {e}");
                return;
            }

            TwitchHelper.SendMessage($"@{username} => {AppConfig.BroadcasterName} has {subCount} subs");
            _subsCooldown = DateTime.UtcNow;
            return;
        }
    }
}