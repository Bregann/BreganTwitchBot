using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.WordBlacklist
{
    public class WordBlacklistMonitorService : IWordBlacklistMonitorService
    {
        internal List<WordBlacklistItem> _wordBlacklist = [];
        internal List<WordBlacklistUser> _wordBlacklistUsers = [];
        private readonly ITwitchHelperService _twitchHelperService;
        static readonly Regex fancyRegex = new Regex(@"[\u2800-\u28FF\u2580-\u259F\u2500-\u257F\u25A0-\u25FF]", RegexOptions.Compiled);

        public WordBlacklistMonitorService(IServiceProvider serviceProvider, ITwitchHelperService twitchHelperService)
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

            _twitchHelperService = twitchHelperService;
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

        public async Task CheckMessageForBlacklistedWords(string message, string userId, string broadcasterId)
        {
            // Check if the message contains any blacklisted words
            var blacklistedWords = _wordBlacklist.Where(x => x.BroadcasterId == broadcasterId && message.ToLower().Contains(x.Word, StringComparison.CurrentCultureIgnoreCase)).ToList();
            // Check the message for any fancy unicode characters
            bool hasFancyCharacters = fancyRegex.IsMatch(message);
            WordType? wordType = null;

            if (hasFancyCharacters)
            {
                wordType = WordType.StrikeWord; // Treat messages with fancy characters as strike words
            }

            if (blacklistedWords.Count != 0)
            {
                foreach (var blacklistedWord in blacklistedWords)
                {
                    // check the wordtype type, if it's greater than the current wordType, set it
                    if (wordType == null || blacklistedWord.WordType > wordType)
                    {
                        wordType = blacklistedWord.WordType;
                    }
                }
            }

            switch (wordType)
            {
                case WordType.TempBanWord:
                    await _twitchHelperService.TimeoutUser(broadcasterId, userId, 300, "You have been timed out for using blacklisted words.");
                    break;
                case WordType.PermBanWord:
                    await _twitchHelperService.BanUser(broadcasterId, userId, "You have been banned for using blacklisted words.");
                    break;
                case WordType.StrikeWord:
                    if (_wordBlacklistUsers.Any(x => x.UserId == userId && x.BroadcasterId == broadcasterId))
                    {
                        await _twitchHelperService.TimeoutUser(broadcasterId, userId, 300, "You have been timed out for using blacklisted words after a warning.");
                        _wordBlacklistUsers.Remove(_wordBlacklistUsers.First(x => x.UserId == userId && x.BroadcasterId == broadcasterId));
                    }
                    else
                    {
                        _wordBlacklistUsers.Add(new WordBlacklistUser
                        {
                            UserId = userId,
                            BroadcasterId = broadcasterId,
                            AddedAt = DateTime.UtcNow
                        });

                        await _twitchHelperService.WarnUser(broadcasterId, userId, "You have been warned for using blacklisted words. Further violations may result in a timeout or ban.");
                    }
                    break;
                case null:
                    // No blacklisted words found, do nothing
                    break;
            }
        }
    }

    public void RemoveWarnedUsers()
    {
        // Remove users who have been warned for more than 5 minutes
        var now = DateTime.UtcNow;
        var usersToRemove = _wordBlacklistUsers.Where(x => (now - x.AddedAt).TotalMinutes > 5).ToList();
        foreach (var user in usersToRemove)
        {
            _wordBlacklistUsers.Remove(user);
        }
    }
}
}
