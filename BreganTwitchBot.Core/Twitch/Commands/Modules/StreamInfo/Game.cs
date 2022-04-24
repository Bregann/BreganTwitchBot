using BreganTwitchBot.Services;
using BreganTwitchBot.Twitch.Helpers;
using Serilog;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.StreamInfo
{
    public class Game
    {
        private static DateTime _gameCooldown;

        public static async Task HandleGameCommand(OnChatCommandReceivedArgs command)
        {
            //if all they are doing is checking the title
            if (command.Command.ChatMessage.Message.ToLower() == "!game")
            {
                if (DateTime.Now - TimeSpan.FromSeconds(5) <= _gameCooldown)
                {
                    Log.Information("[Twitch Commands] !game command handled successfully (cooldown)");
                    return;
                }

                await GetGame(command.Command.ChatMessage.Username);
                _gameCooldown = DateTime.Now;
                return;
            }

            //If updating the command
            if (command.Command.ChatMessage.IsModerator || command.Command.ChatMessage.IsBroadcaster)
            {
                await SetGame(command.Command.ChatMessage.Username, command.Command.ArgumentsAsString);
            }
        }

        private static async Task GetGame(string username)
        {
            GetChannelInformationResponse getChannelRequest;

            try
            {
                getChannelRequest = await TwitchApiConnection.ApiClient.Helix.Channels.GetChannelInformationAsync(Config.TwitchChannelID);
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Commands] Error getting channel - {e}");
                TwitchHelper.SendMessage($"@{username} => hooo I could not get the game sorry try again later");

                return;
            }

            TwitchHelper.SendMessage($"@{username} => The current game is {getChannelRequest.Data[0].GameName}");
        }

        private static async Task SetGame(string username, string game)
        {
            GetChannelInformationResponse getChannelRequest;

            try
            {
                getChannelRequest = await TwitchApiConnection.ApiClient.Helix.Channels.GetChannelInformationAsync(Config.TwitchChannelID);
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

                var request = new TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation.ModifyChannelInformationRequest()
                {
                    Title = getChannelRequest.Data[0].Title,
                    GameId = gameId.Games[0].Id
                };

                await TwitchApiConnection.ApiClient.Helix.Channels.ModifyChannelInformationAsync(Config.TwitchChannelID, request, Config.BroadcasterOAuth);
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
