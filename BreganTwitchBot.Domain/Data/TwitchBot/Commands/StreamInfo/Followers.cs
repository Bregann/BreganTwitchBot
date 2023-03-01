using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo
{
    public class Followers
    {
        private static DateTime _followersCooldown;

        public static async Task HandleFollowersCommand(string username, string userId)
        {
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _followersCooldown && !SuperMods.Supermods.IsUserSupermod(userId))
            {
                Log.Information("[Twitch Commands] !followers command handled successfully (cooldown)");
                return;
            }

            try
            {
                var followerCount = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: AppConfig.TwitchChannelID);
                TwitchHelper.SendMessage($"@{username} => {AppConfig.BroadcasterName} has {followerCount.TotalFollows:N0} followers");
                _followersCooldown = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                TwitchHelper.SendMessage($"@{username} =>  => oh no something broke what a shame 4Head");
                Log.Error($"[Twitch Commands] !followers command errored - {e}");
                _followersCooldown = DateTime.UtcNow;
                return;
            }
        }
    }
}