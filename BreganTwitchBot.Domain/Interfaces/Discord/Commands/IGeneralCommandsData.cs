using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IGeneralCommandsData
    {
        Task<string> AddUserBirthday(AddBirthdayCommand command);

        /// <summary>
        /// Removes the caller's birthday from this server
        /// </summary>
        Task<string> RemoveUserBirthday(ulong guildId, ulong discordUserId);
        Task CheckForUserBirthdaysAndSendMessage();
    }
}
