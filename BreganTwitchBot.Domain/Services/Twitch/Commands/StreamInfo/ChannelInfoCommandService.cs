using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.StreamInfo
{
    public class ChannelInfoCommandService(IServiceProvider serviceProvider)
    {
        private readonly CommandCooldownHelper _titleCooldown = new(TimeSpan.FromSeconds(5));
        private readonly CommandCooldownHelper _gameCooldown = new(TimeSpan.FromSeconds(5));

        [TwitchCommand("title")]
        public async Task TitleCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var channelInfoDataService = scope.ServiceProvider.GetRequiredService<IChannelInfoDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var newTitle = GetArgument(msgParams);

                // "!title" on its own shows the title, "!title <something>" sets it
                if (newTitle == null)
                {
                    var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                    if (!isSuperMod && _titleCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
                    {
                        return;
                    }

                    _titleCooldown.StartCooldown(msgParams.BroadcasterChannelId);

                    var getResponse = await channelInfoDataService.GetTitleAsync(msgParams);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, getResponse, msgParams.MessageId);
                    return;
                }

                try
                {
                    var setResponse = await channelInfoDataService.SetTitleAsync(msgParams, newTitle);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, setResponse, msgParams.MessageId);
                }
                catch (UnauthorizedAccessException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
            }
        }

        [TwitchCommand("game", ["category"])]
        public async Task GameCommand(ChannelChatMessageReceivedParams msgParams)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var channelInfoDataService = scope.ServiceProvider.GetRequiredService<IChannelInfoDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                var newGame = GetArgument(msgParams);

                // "!game" on its own shows the game, "!game <something>" sets it
                if (newGame == null)
                {
                    var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

                    if (!isSuperMod && _gameCooldown.IsOnCooldown(msgParams.BroadcasterChannelId))
                    {
                        return;
                    }

                    _gameCooldown.StartCooldown(msgParams.BroadcasterChannelId);

                    var getResponse = await channelInfoDataService.GetGameAsync(msgParams);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, getResponse, msgParams.MessageId);
                    return;
                }

                try
                {
                    var setResponse = await channelInfoDataService.SetGameAsync(msgParams, newGame);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, setResponse, msgParams.MessageId);
                }
                catch (UnauthorizedAccessException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
            }
        }

        /// <summary>
        /// Everything after the command itself, or null when the command was used on its own.
        /// Whitespace only arguments count as no argument so "!title    " still reads the title
        /// rather than blanking it.
        /// </summary>
        private static string? GetArgument(ChannelChatMessageReceivedParams msgParams)
        {
            var argument = string.Join(' ', msgParams.MessageParts.Skip(1)).Trim();
            return string.IsNullOrWhiteSpace(argument) ? null : argument;
        }
    }
}
