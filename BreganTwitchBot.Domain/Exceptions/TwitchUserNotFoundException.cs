namespace BreganTwitchBot.Domain.Exceptions
{
    public class TwitchUserNotFoundException : Exception
    {
        public TwitchUserNotFoundException()
        {
        }

        public TwitchUserNotFoundException(string message) : base(message)
        {
        }

        public TwitchUserNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
