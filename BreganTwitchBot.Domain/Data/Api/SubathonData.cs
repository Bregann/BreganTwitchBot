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
    }
}
