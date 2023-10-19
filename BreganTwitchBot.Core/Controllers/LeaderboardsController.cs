using BreganTwitchBot.Domain.Data.Api;
using BreganTwitchBot.Domain.Data.Api.Dtos;
using BreganTwitchBot.Domain.Data.Api.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LeaderboardsController : ControllerBase
    {
        [HttpGet("{type}")]
        public GetLeaderboardDto GetLeaderboard([FromRoute] LeaderboardType type)
        {
            if (type == LeaderboardType.AllTimeHours ||
                type == LeaderboardType.MonthlyHours ||
                type == LeaderboardType.WeeklyHours ||
                type == LeaderboardType.StreamHours)
            {
                return new GetLeaderboardDto
                {
                    LeaderboardName = type.ToString(),
                    Leaderboards = LeaderboardsData.GetHourLeaderboard(type)
                };
            }

            return new GetLeaderboardDto
            {
                LeaderboardName = type.ToString(),
                Leaderboards = LeaderboardsData.GetLeaderboard(type)
            };
        }
    }
}
