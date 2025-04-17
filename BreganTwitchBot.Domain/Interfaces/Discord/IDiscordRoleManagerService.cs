namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordRoleManagerService
    {
        Task AddRolesToUserOnGuildJoin(ulong discordUserId, ulong guildId);
        Task AddRolesToUserOnLink(string twitchUserId);
        Task ApplyRoleOnDiscordWatchtimeRankup(string twitchUserId, string broadcasterChannelId);
    }
}
