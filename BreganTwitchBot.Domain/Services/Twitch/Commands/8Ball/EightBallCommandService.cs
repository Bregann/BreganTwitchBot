using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands._8Ball
{
    public class EightBallCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("8ball")]
        public async Task EightBallCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var responses = new List<string>
                {
                    "It is certain",
                    "It is decidedly so",
                    "Without a doubt",
                    "Yes, definitely",
                    "You may rely on it",
                    "As I see it, yes",
                    "Most likely",
                    "Outlook good",
                    "Yes",
                    "Signs point to yes",
                    "Reply hazy try again",
                    "Ask again later",
                    "Better not tell you now",
                    "Cannot predict now",
                    "Concentrate and ask again",
                    "Don't count on it",
                    "My reply is no",
                    "My sources say no",
                    "Outlook not so good",
                    "Very doubtful",
                    "No",
                    "Not a chance",
                    "No way",
                    "Nope. Lol.",
                    "Not in a million years"
                };
                var random = new Random();
                var response = responses[random.Next(responses.Count)];
                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
            }
        }
    }
}
