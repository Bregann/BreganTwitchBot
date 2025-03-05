using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using Hangfire;
using Serilog;

namespace BreganTwitchBot.Domain.Helpers
{
    public class HangfireJobServiceHelper(TwitchApiConnection twitchApiConnection, AppDbContext context)
    {
        private readonly TwitchApiConnection _twitchApiConnection = twitchApiConnection;
        private readonly AppDbContext _context = context;

        public async Task SetupHangfireJobs()
        {
            RecurringJob.AddOrUpdate("BigBenBong", () => BigBenBong(), "0 * * * *");
        }

        public void BigBenBong()
        {
            Log.Information("Big Ben Bong");
        }

    }
}
