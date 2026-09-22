namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordStatusService
    {
        /// <summary>
        /// Sets the bot's Discord presence to the member count of the guild it serves
        /// </summary>
        Task UpdateMemberCountStatusAsync();
    }
}
