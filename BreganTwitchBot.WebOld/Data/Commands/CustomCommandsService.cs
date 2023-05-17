using BreganTwitchBot.Infrastructure.Database.Context;

namespace BreganTwitchBot.Web.Data.Commands
{
    public class CustomCommandsService
    {
        public List<CustomCommands> GetCommands()
        {
            using (var context = new DatabaseContext())
            {
                var commands = context.Commands.ToList();

                return (commands.Select(command => new CustomCommands
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