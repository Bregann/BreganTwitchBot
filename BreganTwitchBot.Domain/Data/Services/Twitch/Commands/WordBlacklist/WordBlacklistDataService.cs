using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.WordBlacklist
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
            var wordToAdd = msgParams.MessageParts[1].ToLower().Trim();

            var existingWord = await context.Blacklist.AnyAsync(x => x.Word == wordToAdd && x.WordType == wordType && x.Channel.BroadcasterTwitchChannelName == msgParams.BroadcasterChannelName);

            if (existingWord)
            {
                throw new InvalidCommandException($"The word is already in the blacklist!");
            }

            // add the word to the database
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelName == msgParams.BroadcasterChannelName);

            context.Blacklist.Add(new Blacklist
            {
                Word = wordToAdd,
                WordType = wordType,
                ChannelId = channel.Id
            });

            await context.SaveChangesAsync();
            wordBlacklistMonitorService.AddWordToBlacklist(msgParams.BroadcasterChannelId, wordToAdd, wordType);

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
            var itemsRemoved = await context.Blacklist.Where((x => x.Word == wordToRemove && x.WordType == wordType && x.Channel.BroadcasterTwitchChannelName == msgParams.BroadcasterChannelName)).ExecuteDeleteAsync();

            if (itemsRemoved == 0)
            {
                throw new InvalidCommandException($"The word is not in the blacklist!");
            }

            wordBlacklistMonitorService.RemoveWordFromBlacklist(wordToRemove, msgParams.BroadcasterChannelId);
            return $"The word has been removed from the blacklist!";
        }
    }
}
