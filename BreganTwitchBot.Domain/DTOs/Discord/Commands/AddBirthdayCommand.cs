using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Discord.Commands
{
    public class AddBirthdayCommand : DiscordCommand
    {
        public required int Day { get; set; }
        public required int Month { get; set; }
    }
}
