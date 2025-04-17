using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.WordBlacklist
{
    public class WordBlacklistCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("addstrikeword")]
        public async Task HandleAddStrikeWordCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleAddWordCommand(msgParams, WordType.StrikeWord);
        }

        [TwitchCommand("addtempbanword")]
        public async Task HandleAddTempBanWordCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleAddWordCommand(msgParams, WordType.TempBanWord);
        }

        [TwitchCommand("addpermbanword", ["addbadword"])]
        public async Task HandleAddPermBanWordCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleAddWordCommand(msgParams, WordType.PermBanWord);
        }

        [TwitchCommand("removestrikeword")]
        public async Task HandleRemoveStrikeWordCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleRemoveWordCommand(msgParams, WordType.StrikeWord);
        }

        [TwitchCommand("removetempbanword")]
        public async Task HandleRemoveTempBanWordCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleRemoveWordCommand(msgParams, WordType.TempBanWord);
        }

        [TwitchCommand("removepermbanword", ["removebadword"])]
        public async Task HandleRemovePermBanWordCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleRemoveWordCommand(msgParams, WordType.PermBanWord);
        }

        private async Task HandleAddWordCommand(ChannelChatMessageReceivedParams msgParams, WordType wordType)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var wordBlacklistDataService = scope.ServiceProvider.GetRequiredService<IWordBlacklistDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                try
                {
                    var response = await wordBlacklistDataService.HandleAddWordCommand(msgParams, wordType);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
                }
                catch (InvalidCommandException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
            }
        }

        private async Task HandleRemoveWordCommand(ChannelChatMessageReceivedParams msgParams, WordType wordType)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var wordBlacklistDataService = scope.ServiceProvider.GetRequiredService<IWordBlacklistDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                try
                {
                    var response = await wordBlacklistDataService.HandleRemoveWordCommand(msgParams, wordType);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
                }
                catch (InvalidCommandException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
            }
        }
    }
}
