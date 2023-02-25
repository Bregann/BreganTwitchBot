using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Events
{
    public class Raid
    {
        public static void HandleRaidEvent(string viewCount, string raiderName)
        {
            var succesful = int.TryParse(viewCount, out var raidAmount);

            if (!succesful)
            {
                TwitchHelper.SendMessage($"Thank you {raiderName} for the raid! Make sure to check out {raiderName} at twitch.tv/{raiderName} !");
            }

            if (raidAmount > 50)
            {
                HangfireJobs.StartRaidFollowersOffJob();
            }

            TwitchHelper.SendMessage($"Welcome {raiderName} and their humble {viewCount} raiders! Make sure to check out {raiderName} at twitch.tv/{raiderName} !");
        }
    }
}
