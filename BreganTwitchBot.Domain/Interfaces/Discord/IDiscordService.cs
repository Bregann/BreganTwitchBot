namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordService
    {
        Task StartAsync();
        Task StopAsync();
    }
}
