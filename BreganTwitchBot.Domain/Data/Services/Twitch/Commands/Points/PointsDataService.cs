using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points
{
    public class PointsDataService(AppDbContext dbContext) : IPointsDataService
    {
        private readonly AppDbContext _context = dbContext;

        public async Task<int> GetPointsAsync(string twitchUserId)
        {
            // do some cool stuff
            return 0;
        }
    }
}
