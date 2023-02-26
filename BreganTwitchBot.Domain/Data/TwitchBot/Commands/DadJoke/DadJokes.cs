using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using ICanHazDadJoke.NET;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.DadJoke
{
    public class DadJokes
    {
        public static async Task HandleDadJokeCommand(string username)
        {
            try
            {
                //Make dad bot work and return a joke generated
                var libraryName = "ICanHazDadJoke.NET Readme";
                var contactUri = "https://github.com/mattleibow/ICanHazDadJoke.NET";
                var dadClient = new DadJokeClient(libraryName, contactUri);
                TwitchHelper.SendMessage($"@{username} => {await dadClient.GetRandomJokeStringAsync()}");
                Log.Information("[Twitch Commands] !dadjoke command handled successfully");
            }
            catch (Exception e)
            {
                Log.Warning($"[Twitch Command] !dadjoke command errored - {e}");
                TwitchHelper.SendMessage("Oh no your dad joke command broke me :( try again");
                return;
            }
        }
    }
}