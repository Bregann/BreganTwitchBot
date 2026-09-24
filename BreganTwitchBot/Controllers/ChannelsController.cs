using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelsController(IApiDataService apiDataService) : ControllerBase
    {
        /// <summary>
        /// Headline numbers for a channel
        /// </summary>
        [HttpGet("{broadcasterChannelName}")]
        [ProducesResponseType(typeof(GetChannelSummaryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChannelSummary([FromRoute] string broadcasterChannelName)
        {
            var summary = await apiDataService.GetChannelSummaryAsync(broadcasterChannelName);

            return summary == null ? NotFound() : Ok(summary);
        }

        /// <summary>
        /// Past streams for a channel, most recent first
        /// </summary>
        [HttpGet("{broadcasterChannelName}/streams")]
        [ProducesResponseType(typeof(List<GetStreamHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStreamHistory([FromRoute] string broadcasterChannelName, [FromQuery] int take = 30)
        {
            if (take is < 1 or > 200)
            {
                return BadRequest("take must be between 1 and 200");
            }

            var streams = await apiDataService.GetStreamHistoryAsync(broadcasterChannelName, take);

            return streams == null ? NotFound() : Ok(streams);
        }
    }
}
