using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Serilog;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo
{
    public class Title
    {
        private static DateTime _titleCooldown;

        public static async Task HandleTitleCommand(string username, string chatMessage, string chatArgumentsAsString, bool isMod, bool isBroadcaster)
        {
            //if all they are doing is checking the title
            if (chatMessage.ToLower() == "!title")
            {
                if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _titleCooldown)
                {
                    Log.Information("[Twitch Commands] !title command handled successfully (cooldown)");
                    return;
                }

                await GetTitle(username);
                _titleCooldown = DateTime.UtcNow;
                return;
            }

            //If updating the command
            if (isMod || isBroadcaster)
            {
                await SetChannelTitle(username, chatArgumentsAsString);
            }
        }

        private static async Task GetTitle(string username)
        {
            try
            {
                var getChannelRequest = await TwitchApiConnection.ApiClient.Helix.Channels.GetChannelInformationAsync(AppConfig.TwitchChannelID);
                TwitchHelper.SendMessage($"@{username} => The current stream title is {getChannelRequest.Data[0].Title}");
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting channel - {e}");
                TwitchHelper.SendMessage($"@{username} => hooo I could not get the channel title sorry try again later");
                return;
            }
        }

        private static async Task SetChannelTitle(string username, string title)
        {
            GetChannelInformationResponse getChannelRequest;

            try
            {
                getChannelRequest = await TwitchApiConnection.ApiClient.Helix.Channels.GetChannelInformationAsync(AppConfig.TwitchChannelID);
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting channel - {e}");
                TwitchHelper.SendMessage($"@{username} => hooo I could not get the channel title sorry try again later");
                return;
            }

            try
            {
                var request = new ModifyChannelInformationRequest()
                {
                    Title = title,
                    GameId = getChannelRequest.Data[0].GameId
                };

                await TwitchApiConnection.ApiClient.Helix.Channels.ModifyChannelInformationAsync(AppConfig.TwitchChannelID, request, AppConfig.BroadcasterOAuth);
                TwitchHelper.SendMessage($"@{username} => The stream title has been updated to {title} and the current game is {getChannelRequest.Data[0].GameName} :)");
                return;
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error updating tite - {e}");
                TwitchHelper.SendMessage($"@{username} => well that broke (couldn't update the title)");
                return;
            }
        }
    }
}