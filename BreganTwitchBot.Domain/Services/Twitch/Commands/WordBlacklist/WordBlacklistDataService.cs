using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.WordBlacklist
{
    public class WordBlacklistDataService(AppDbContext context, ITwitchHelperService twitchHelperService, IWordBlacklistMonitorService wordBlacklistMonitorService) : IWordBlacklistDataService
    {
        public async Task<string> HandleAddWordCommand(ChannelChatMessageReceivedParams msgParams, WordType wordType)
        {
            // check if command user is a super mod or broadcaster
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            if (!isSuperMod && !msgParams.IsBroadcaster)
            {
                throw new InvalidCommandException("You do not have permission to use this command.");
            }

            // check if they've ran the command properly
            if (msgParams.MessageParts.Length < 2)
            {
                throw new InvalidCommandException("You must specify a word to add. Duhhh");
            }

            // check if the word is already in the database
            var wordToAdd = string.Join(" ", msgParams.MessageParts.Skip(1)).ToLower().Trim();

            var existingWord = await context.Blacklist.AnyAsync(x => x.Word == wordToAdd && x.WordType == wordType && x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);

            if (existingWord)
            {
                throw new InvalidCommandException($"The word is already in the blacklist!");
            }

            // add the word to the database
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);

            context.Blacklist.Add(new Blacklist
            {
                Word = wordToAdd,
                WordType = wordType,
                ChannelId = channel.Id
            });

            await context.SaveChangesAsync();
            wordBlacklistMonitorService.AddWordToBlacklist(wordToAdd, msgParams.BroadcasterChannelId, wordType);

            return $"The word has been added to the blacklist!";
        }

        public async Task<string> HandleRemoveWordCommand(ChannelChatMessageReceivedParams msgParams, WordType wordType)
        {
            // check if command user is a super mod or broadcaster
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);
            if (!isSuperMod && !msgParams.IsBroadcaster)
            {
                throw new InvalidCommandException("You do not have permission to use this command.");
            }

            // check if they've ran the command properly
            if (msgParams.MessageParts.Length < 2)
            {
                throw new InvalidCommandException("You must specify a word to remove. Duhhh");
            }

            // check if the word is already in the database
            var wordToRemove = msgParams.MessageParts[1].ToLower().Trim();

            // remove the word from the database
            var itemsRemoved = await context.Blacklist.Where((x => x.Word == wordToRemove && x.WordType == wordType && x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId)).ExecuteDeleteAsync();

            if (itemsRemoved == 0)
            {
                throw new InvalidCommandException($"The word is not in the blacklist!");
            }

            wordBlacklistMonitorService.RemoveWordFromBlacklist(wordToRemove, msgParams.BroadcasterChannelId);
            return $"The word has been removed from the blacklist!";
        }
    }
}
