using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IWordBlacklistMonitorService
    {
        (bool IsBlacklisted, WordType? BlacklistType) IsWordBlacklisted(string word, string broadcasterId);
        void AddWordToBlacklist(string word, string broadcasterId, WordType wordType);
        void RemoveWordFromBlacklist(string word, string broadcasterId);
        Task CheckMessageForBlacklistedWords(string message, string userId, string broadcasterId);
        void RemoveWarnedUsers();
    }
}
