namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IPointsDataService
    {
        Task<int> GetPointsAsync(string twitchUserId);
    }
}
