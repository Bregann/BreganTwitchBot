using BreganTwitchBot.Services;
using BreganTwitchBot.Twitch.Helpers;
using Serilog;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.StreamInfo
{
    public class Followers
    {
        private static DateTime _followersCooldown;
        public static async Task HandleFollowersCommand(string username, string userId)
        {
            if (DateTime.Now - TimeSpan.FromSeconds(5) <= _followersCooldown && !SuperMods.SuperMods.IsUserSupermod(userId))
            {
                Log.Information("[Twitch Commands] !followers command handled successfully (cooldown)");
                return;
            }

            try
            {
                var followerCount = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: Config.TwitchChannelID);
                TwitchHelper.SendMessage($"@{username} => {Config.BroadcasterName} has {followerCount.TotalFollows:N0} followers");
                _followersCooldown = DateTime.Now;
            }
            catch (Exception e)
            {
                TwitchHelper.SendMessage($"@{username} =>  => oh no something broke what a shame 4Head");
                Log.Error($"[Twitch Commands] !followers command errored - {e}");
                _followersCooldown = DateTime.Now;
                return;
            }
        }
    }
}
