using BreganTwitchBot.Services;
using Serilog;
using TwitchLib.Client.Extensions;

namespace BreganTwitchBot.Twitch.Helpers
{
    public class TwitchHelper
    {
        public static void SendMessage(string messageToSend)
        {
            try
            {
                TwitchBotConnection.Client.SendMessage(Config.BroadcasterName, messageToSend);
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
                TwitchBotConnection.Client.TimeoutUser(Config.BroadcasterName, user, timeToTimeoutFor, reason);
                Log.Information($"[Twitch Timeout] {user} timed out for {timeToTimeoutFor.TotalSeconds}");
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Timeout] Error timing out user - {e}");
            }
        }

        public static void BanUser(string user, string reason)
        {
            try
            {
                TwitchBotConnection.Client.BanUser(Config.BroadcasterName, user, reason);
                Log.Information($"[Twitch Ban] {user} permanently banned from the twitch dot television livestream");
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
                TwitchBotConnection.Client.DeleteMessage(Config.BroadcasterName, msgId);
                Log.Information($"[Twitch Ban] Message ID {msgId} deleted");
            }
            catch (Exception e)
            {
                Log.Error($"[Twitch Message Delete] Error deleting message - {e}");
            }
        }
    }
}
