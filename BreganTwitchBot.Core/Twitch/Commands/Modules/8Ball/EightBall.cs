using BreganTwitchBot.Twitch.Helpers;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.EightBall
{
    public class EightBall
    {
        public static void Handle8BallCommand(string username)
        {
            var rng = new Random();
            var randomSelection = rng.Next(0, 18);
            var randomList = new List<string>{"Not Likely", "Without a doubt","Better not tell you now", "Don't count on it", "It seems certain", "Ask again later",
                "Absolutely", "Maybe later", "Not today", "There is a chance", "You may rely on it", "Outlook seems good","My sources say no",
                "There is a good chance", "Yes", "Not in a million years", "As I see it, yes", "No", "Outlook not so good"};

            TwitchHelper.SendMessage($"@{username} => {randomList[randomSelection]}");
        }
    }
}
