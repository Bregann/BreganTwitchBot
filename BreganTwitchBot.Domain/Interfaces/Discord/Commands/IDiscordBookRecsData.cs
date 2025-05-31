using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordBookRecsData
    {
        Task AddNewLikedItem(ulong discordUserId, string items, AiType likedItemType);
        Task<string[]> AnalyseBooks(ulong discordUserId, string imageUrl, string fileType, bool isGemini);
        Task RemoveLikedItem(ulong discordUserId, string item);
    }
}
