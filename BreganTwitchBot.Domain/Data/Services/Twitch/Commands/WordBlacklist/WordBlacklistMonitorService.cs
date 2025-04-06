using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.WordBlacklist
{
    public class WordBlacklistMonitorService : IWordBlacklistMonitorService
    {
        internal List<WordBlacklistItem> _wordBlacklist = new List<WordBlacklistItem>();
        public WordBlacklistMonitorService(IServiceProvider serviceProvider)
        {
            // Load the words from the database
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _wordBlacklist = context.Blacklist
                    .Select(x => new WordBlacklistItem
                    {
                        Word = x.Word,
                        WordType = x.WordType,
                        BroadcasterId = x.Channel.BroadcasterTwitchChannelId,
                    })
                    .ToList();
            }
        }

        public (bool IsBlacklisted, WordType? BlacklistType) IsWordBlacklisted(string word, string broadcasterId)
        {
            // Check if the word is blacklisted for the broadcaster
            var isBlacklisted = _wordBlacklist.FirstOrDefault(x => x.Word == word && x.BroadcasterId == broadcasterId);

            if (isBlacklisted != null)
            {
                return (true, isBlacklisted.WordType);
            }
            else
            {
                return (false, null);
            }
        }

        public void AddWordToBlacklist(string word, string broadcasterId, WordType wordType)
        {
            // Add the word to the blacklist
            _wordBlacklist.Add(new WordBlacklistItem
            {
                Word = word,
                BroadcasterId = broadcasterId,
                WordType = wordType
            });
        }

        public void RemoveWordFromBlacklist(string word, string broadcasterId)
        {
            // Remove the word from the blacklist
            var wordToRemove = _wordBlacklist.FirstOrDefault(x => x.Word == word && x.BroadcasterId == broadcasterId);
            if (wordToRemove != null)
            {
                _wordBlacklist.Remove(wordToRemove);
            }
        }
    }
}
