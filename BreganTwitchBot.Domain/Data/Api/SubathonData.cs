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
            var timeUpdated = false;
            var playSound = false;

            if (_subathonTime != AppConfig.SubathonTime)
            {
                timeUpdated = true;

                if (_subathonTime.TotalSeconds + 60 < AppConfig.SubathonTime.TotalSeconds)
                {
                    playSound = true;
                }

            }

            _subathonTime = AppConfig.SubathonTime;

            var startTime = AppConfig.SubathonStartTime;
            var endTimeDT = startTime.Add(AppConfig.SubathonTime);
            var timeDiff = endTimeDT - DateTime.UtcNow;

            if (timeDiff.TotalMicroseconds < 0)
            {
                return new GetSubathonTimeLeftDto
                {
                    SecondsLeft = 0,
                    PlaySound = false,
                    TimeUpdated = true
                };
            }

            return new GetSubathonTimeLeftDto
            {
                SecondsLeft = (int)Math.Round(timeDiff.TotalSeconds),
                PlaySound = playSound,
                TimeUpdated = timeUpdated
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
