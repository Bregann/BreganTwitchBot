
namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.CustomCommands
{
    [Serializable]
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException()
        {
        }

        public CommandNotFoundException(string? message) : base(message)
        {
        }

        public CommandNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}