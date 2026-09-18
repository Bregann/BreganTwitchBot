using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubathonController(IApiDataService apiDataService) : ControllerBase
    {
        /// <summary>
        /// Gets the state of a channel's subathon, including how long is left
        /// </summary>
        [HttpGet("{broadcasterChannelName}/status")]
        [ProducesResponseType(typeof(GetSubathonStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubathonStatus([FromRoute] string broadcasterChannelName)
        {
            var status = await apiDataService.GetSubathonStatusAsync(broadcasterChannelName);

            return status == null ? NotFound() : Ok(status);
        }

        /// <summary>
        /// Gets who has contributed most to a channel's subathon
        /// </summary>
        [HttpGet("{broadcasterChannelName}/leaderboard")]
        [ProducesResponseType(typeof(GetSubathonLeaderboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubathonLeaderboard([FromRoute] string broadcasterChannelName, [FromQuery] int take = 10)
        {
            if (take is < 1 or > 100)
            {
                return BadRequest("take must be between 1 and 100");
            }

            var leaderboard = await apiDataService.GetSubathonLeaderboardAsync(broadcasterChannelName, take);

            return leaderboard == null ? NotFound() : Ok(leaderboard);
        }
    }
}
