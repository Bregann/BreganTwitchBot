using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Humanizer;
using Humanizer.Localisation;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.Subathon
{
    public class Subathon
    {
        private static DateTime _subathonCooldown;

        public static void HandleSubathonCommand(string username)
        {
            //5 sec cooldown to prevent spam
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _subathonCooldown && !TwitchHelper.IsUserSupermod(username))
            {
                return;
            }

            if (!AppConfig.SubathonActive)
            {
                return;
            }

            //Get the stream uptime
            var startTime = new DateTime(2022, 9, 25, 11, 35, 0);

            var endTimeDT = startTime.Add(AppConfig.SubathonTime);
            var timeLeft = endTimeDT - DateTime.UtcNow;

            TwitchHelper.SendMessage($"@{username} => The subathon has been extended to a total of {AppConfig.SubathonTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}! The stream will end in {timeLeft.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}. See all the info at https://bot.bregan.me/subathon");
            _subathonCooldown = DateTime.UtcNow;
        }
    }
}
