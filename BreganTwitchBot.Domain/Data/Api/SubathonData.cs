using BreganTwitchBot.Domain.Data.Api.Dtos;
using BreganTwitchBot.Infrastructure.Database.Context;
using Humanizer;
using Humanizer.Localisation;

namespace BreganTwitchBot.Domain.Data.Api
{
    public class SubathonData
    {
        public static GetSubathonTimeLeftDto GetSubathonTimeLeft()
        {
            var startTime = AppConfig.SubathonStartTime;
            var endTimeDT = startTime.Add(AppConfig.SubathonTime);
            var timeDiff = endTimeDT - DateTime.UtcNow;

            var timeUpdated = false;
            var playSound = AppConfig.PlaySubathonSound;

            if (AppConfig.PrevSubathonTime.TotalSeconds < AppConfig.SubathonTime.TotalSeconds)
            {
                timeUpdated = true;
            }

            if (timeDiff.TotalMicroseconds < 0)
            {
                return new GetSubathonTimeLeftDto
                {
                    SecondsLeft = 0,
                    PlaySound = false,
                    TimeUpdated = true,
                    TimeExtended = AppConfig.SubathonTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)
                };
            }

            AppConfig.ToggleSubathonSoundOff();

            return new GetSubathonTimeLeftDto
            {
                SecondsLeft = (int)Math.Round(timeDiff.TotalSeconds),
                PlaySound = playSound,
                TimeUpdated = timeUpdated,
                TimeExtended = AppConfig.SubathonTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)
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
                    BitsLeaderboard = bitsLb,
                    SubsLeaderboard = subsLb
                };
            }
        }
    }
}
