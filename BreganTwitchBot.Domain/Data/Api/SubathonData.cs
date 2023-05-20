using BreganTwitchBot.Domain.Data.Api.Dtos;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo;
using BreganTwitchBot.Infrastructure.Database.Context;
using Humanizer;
using Humanizer.Localisation;

namespace BreganTwitchBot.Domain.Data.Api
{
    public class SubathonData
    {
        public static string GetSubathonTimeLeft()
        {
            var startTime = AppConfig.SubathonStartTime;
            var endTimeDT = startTime.Add(AppConfig.SubathonTime);
            var timeDiff = endTimeDT - DateTime.Now;

            if (timeDiff.TotalMicroseconds < 0)
            {
                return "Subathon is over!";
            }

            return timeDiff.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7);
        }

        public static GetSubathonLeaderboardsDto GetSubathonLeaderboards()
        {
            using (var context = new DatabaseContext())
            {
                var subsLb = context.Subathon.OrderByDescending(x => x.SubsGifted).Take(10).ToList();
                var bitsLb = context.Subathon.OrderByDescending(x => x.BitsDonated).Take(10).ToList();

                return new GetSubathonLeaderboardsDto 
                { 
                    BitsLeaderboard= bitsLb, 
                    SubsLeaderboard = subsLb 
                };
            }
        }
    }
}
