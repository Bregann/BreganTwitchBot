using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardsController(IApiDataService apiDataService) : ControllerBase
    {
        /// <summary>
        /// Gets a leaderboard for a channel
        /// </summary>
        [HttpGet("{broadcasterChannelName}/{type}")]
        [ProducesResponseType(typeof(GetLeaderboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLeaderboard([FromRoute] string broadcasterChannelName, [FromRoute] DiscordLeaderboardType type, [FromQuery] int take = 250)
        {
            if (take is < 1 or > 1000)
            {
                return BadRequest("take must be between 1 and 1000");
            }

            var leaderboard = await apiDataService.GetLeaderboardAsync(broadcasterChannelName, type, take);

            return leaderboard == null ? NotFound() : Ok(leaderboard);
        }
    }
}
