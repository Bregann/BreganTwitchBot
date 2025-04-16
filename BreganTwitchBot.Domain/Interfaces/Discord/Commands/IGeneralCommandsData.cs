using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IGeneralCommandsData
    {
        Task<string> AddUserBirthday(AddBirthdayCommand command);
    }
}
