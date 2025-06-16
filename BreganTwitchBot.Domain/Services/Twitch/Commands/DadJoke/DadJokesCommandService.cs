using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using ICanHazDadJoke.NET;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.DadJoke
{
    public class DadJokesCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("dadjoke", ["joke", "yoke"])]
        public async Task DadJokeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var libraryName = "BreganTwitchBot";
                var contactUri = "https://github.com/Bregann/BreganTwitchBot";
                var dadClient = new DadJokeClient(libraryName, contactUri);

                var dadJoke = await dadClient.GetRandomJokeStringAsync();

                await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, dadJoke, msgParams.MessageId);
            }
        }
    }
}
