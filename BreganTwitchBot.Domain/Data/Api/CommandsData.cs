using BreganTwitchBot.Domain.Data.Api.Dtos;
using BreganTwitchBot.Infrastructure.Database.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Api
{
    public class CommandsData
    {
        public static List<CustomCommandsDto> GetCommands()
        {
            using (var context = new DatabaseContext())
            {
                var commands = context.Commands.ToList();

                return (commands.Select(command => new CustomCommandsDto
                {
                    CommandName = command.CommandName,
                    CommandText = command.CommandText,
                    LastUsed = command.LastUsed,
                    TimesUsed = command.TimesUsed
                })).ToList();
            }
        }
    }
}
