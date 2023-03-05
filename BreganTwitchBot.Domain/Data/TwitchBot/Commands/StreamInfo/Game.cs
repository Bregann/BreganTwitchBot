using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Serilog;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo
{
    public class Game
    {
        private static DateTime _gameCooldown;

        public static async Task HandleGameCommand(string username, string chatMessage, string chatArgumentsAsString, bool isMod, bool isBroadcaster)
        {
            //if all they are doing is checking the title
            if (chatMessage == "!game")
            {
                if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _gameCooldown)
                {
                    Log.Information("[Twitch Commands] !game command handled successfully (cooldown)");
                    return;
                }

                await GetGame(username);
                _gameCooldown = DateTime.UtcNow;
                return;
            }

            //If updating the command
            if (isMod || isBroadcaster)
            {
                await SetGame(username, chatArgumentsAsString);
            }
        }

        private static async Task GetGame(string username)
        {
            try
            {
                var getChannelRequest = await TwitchApiConnection.ApiClient.Helix.Channels.GetChannelInformationAsync(AppConfig.TwitchChannelID);
                TwitchHelper.SendMessage($"@{username} => The current game is {getChannelRequest.Data[0].GameName}");
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting channel - {e}");
                TwitchHelper.SendMessage($"@{username} => hooo I could not get the game sorry try again later");

                return;
            }
        }

        private static async Task SetGame(string username, string game)
        {
            GetChannelInformationResponse getChannelRequest;

            try
            {
                getChannelRequest = await TwitchApiConnection.ApiClient.Helix.Channels.GetChannelInformationAsync(AppConfig.TwitchChannelID);
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting channel - {e}");
                TwitchHelper.SendMessage($"@{username} => hooo I could not get the channel sorry try again later");
                return;
            }

            try
            {
                var gameId = await TwitchApiConnection.ApiClient.Helix.Games.GetGamesAsync(gameNames: new List<string> { game });

                if (gameId.Games == null)
                {
                    TwitchHelper.SendMessage($"@{username} => hooo I could not get the game sorry try again later");
                    return;
                }

                var request = new ModifyChannelInformationRequest()
                {
                    Title = getChannelRequest.Data[0].Title,
                    GameId = gameId.Games[0].Id
                };

                await TwitchApiConnection.ApiClient.Helix.Channels.ModifyChannelInformationAsync(AppConfig.TwitchChannelID, request, AppConfig.BroadcasterOAuth);
                TwitchHelper.SendMessage($"@{username} => The Game been updated to {game} and the current title is {getChannelRequest.Data[0].Title} :)");
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