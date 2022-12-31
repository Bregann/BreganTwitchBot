using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Bot.Twitch.Services;
using Serilog;
using TwitchLib.Client.Extensions;

namespace BreganTwitchBot.Domain.Bot.Twitch.Helpers
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

        public static void TimeoutUser(string user, TimeSpan timeToTimeoutFor, string reason)
        {
            try
            {
                TwitchBotConnection.Client.TimeoutUser(AppConfig.BroadcasterName, user, timeToTimeoutFor, reason);
                Log.Information($"[Twitch Timeout] {user} timed out for {timeToTimeoutFor.TotalSeconds} seconds");
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Timeout] Error timing out user - {e}");
            }
        }

        public static async Task BanUser(string user, string reason)
        {
            try
            {
                TwitchBotConnection.Client.BanUser(AppConfig.BroadcasterName, user, reason);
                Log.Information($"[Twitch Ban] {user} permanently banned from the twitch dot television livestream");

                //Send over to discord for an inspection
                await DiscordHelper.InformDiscordIfUserBanned(user);
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Ban] Error banning user - {e}");
            }
        }

        public static void DeleteMessage(string msgId)
        {
            try
            {
                TwitchBotConnection.Client.DeleteMessage(AppConfig.BroadcasterName, msgId);
                Log.Information($"[Twitch Ban] Message ID {msgId} deleted");
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Message Delete] Error deleting message - {e}");
            }
        }
    }
}