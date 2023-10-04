using BreganTwitchBot.Domain.Data.Api.Dtos;
using BreganTwitchBot.Infrastructure.Database.Context;

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
                    LastUsed = command.LastUsed.ToString(),
                    TimesUsed = command.TimesUsed
                })).ToList();
            }
        }
    }
}
