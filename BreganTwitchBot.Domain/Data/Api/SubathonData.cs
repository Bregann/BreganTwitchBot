using BreganTwitchBot.Domain.Data.Api.Dtos;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo;
using BreganTwitchBot.Infrastructure.Database.Context;
using Humanizer;
using Humanizer.Localisation;

namespace BreganTwitchBot.Domain.Data.Api
{
    public class SubathonData
    {
        private static TimeSpan _subathonTime = AppConfig.SubathonTime;

        public static GetSubathonTimeLeftDto GetSubathonTimeLeft()
        {
            _subathonTime = AppConfig.SubathonTime;

            var startTime = AppConfig.SubathonStartTime;
            var endTimeDT = startTime.Add(AppConfig.SubathonTime);
            var timeDiff = endTimeDT - DateTime.Now;

            if (timeDiff.TotalMicroseconds < 0)
            {
                return new GetSubathonTimeLeftDto
                {
                    TimeLeft = "Subathon is over!",
                    PlaySound = false
                };
            }

            return new GetSubathonTimeLeftDto
            {
                TimeLeft = timeDiff.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7),
                PlaySound = timeDiff != _subathonTime
            };
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
