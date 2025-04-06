using BreganTwitchBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IWordBlacklistMonitorService
    {
        (bool IsBlacklisted, WordType? BlacklistType) IsWordBlacklisted(string word, string broadcasterId);
        void AddWordToBlacklist(string word, string broadcasterId, WordType wordType);
        void RemoveWordFromBlacklist(string word, string broadcasterId);
    }
}
