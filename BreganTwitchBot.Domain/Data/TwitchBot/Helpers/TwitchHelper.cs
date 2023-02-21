using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Bot.Twitch.Services;
using Serilog;
using TwitchLib.Client.Extensions;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Helpers
{
    public class TwitchHelper
    {
        public static void SendMessage(string messageToSend)
        {
            try
            {
                TwitchBotConnection.Client.SendMessage(AppConfig.BroadcasterName, messageToSend);
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Message Sent] Error sending Twitch message - {e}");
            }
        }

        public static async Task TimeoutUser(string userId, int timeToTimeoutFor, string reason)
        {
            try
            {
                var banRequest = new BanUserRequest
                {
                    UserId = userId,
                    Reason = reason,
                    Duration = timeToTimeoutFor
                };


                await TwitchApiConnection.ApiClient.Helix.Moderation.BanUserAsync(AppConfig.TwitchChannelID, AppConfig.BotChannelId, banRequest, AppConfig.TwitchBotApiKey);
                Log.Information($"[Twitch Timeout] {userId} timed out for {timeToTimeoutFor} seconds");
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Timeout] Error timing out user - {e}");
            }
        }

        public static async Task BanUser(string userId, string reason)
        {
            try
            {
                var banRequest = new BanUserRequest
                {
                    UserId = userId,
                    Reason = reason
                };


                await TwitchApiConnection.ApiClient.Helix.Moderation.BanUserAsync(AppConfig.TwitchChannelID, AppConfig.BotChannelId, banRequest, AppConfig.TwitchBotApiKey);
                Log.Information($"[Twitch Ban] {userId} permanently banned from the twitch dot television livestream");

                //Send over to discord for an inspection
                await DiscordHelper.InformDiscordIfUserBanned(userId);
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Ban] Error banning user - {e}");
            }
        }

        public static async Task DeleteMessage(string msgId)
        {
            try
            {
                await TwitchApiConnection.ApiClient.Helix.Moderation.DeleteChatMessagesAsync(AppConfig.TwitchChannelID, AppConfig.BotChannelId, msgId, AppConfig.TwitchBotApiKey);
                Log.Information($"[Twitch Ban] Message ID {msgId} deleted");
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Message Delete] Error deleting message - {e}");
            }
        }
    }
}