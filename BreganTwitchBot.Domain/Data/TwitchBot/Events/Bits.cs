using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Events
{
    public class Bits
    {
        public static async Task HandleBitsEvent(string userId, string username, int bitsUsedThisCheer, int totalBitsCheeredByUser)
        {
            try
            {
                Subathon.AddSubathonBitsTime(bitsUsedThisCheer, username);

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                    if (user != null)
                    {
                        user.BitsDonatedThisMonth += bitsUsedThisCheer;
                        await context.SaveChangesAsync();
                    }
                }

                if (bitsUsedThisCheer <= 4)
                {
                    Log.Information($"[Bits] Just received {bitsUsedThisCheer} bits from {username}. That brings their total to {totalBitsCheeredByUser} bits!");
                    return;
                }

                TwitchHelper.SendMessage($"{username} has donated {bitsUsedThisCheer:N0} bits with a grand total of {totalBitsCheeredByUser} donated PogChamp");
                Log.Information($"[Bits] Just received {bitsUsedThisCheer} bits from {username}. That brings their total to {totalBitsCheeredByUser} bits!");
            }
            catch (Exception ex)
            {
                Log.Warning($"[Bits] {ex}");
                return;
            }
        }
    }
}
