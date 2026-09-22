namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface ITimedMessageService
    {
        /// <summary>
        /// Posts any timed messages that are due, across every channel
        /// </summary>
        Task SendDueMessagesAsync();
    }
}
