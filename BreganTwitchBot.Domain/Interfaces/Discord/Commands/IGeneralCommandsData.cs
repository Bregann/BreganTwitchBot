using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IGeneralCommandsData
    {
        Task<string> AddUserBirthday(AddBirthdayCommand command);
        Task CheckForUserBirthdaysAndSendMessage();
    }
}
