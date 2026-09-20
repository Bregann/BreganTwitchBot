using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using ICanHazDadJoke.NET;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.DadJoke
{
    public class DadJokesCommandService(ITwitchHelperService twitchHelperService)
    {
        [TwitchCommand("dadjoke", ["joke", "yoke"])]
        public async Task DadJokeCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var libraryName = "BreganTwitchBot";
            var contactUri = "https://github.com/Bregann/BreganTwitchBot";
            var dadClient = new DadJokeClient(libraryName, contactUri);

            var dadJoke = await dadClient.GetRandomJokeStringAsync();

            await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, dadJoke, msgParams.MessageId);
        }
    }
}
