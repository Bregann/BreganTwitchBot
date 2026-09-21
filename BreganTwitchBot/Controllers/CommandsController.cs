using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController(IApiDataService apiDataService) : ControllerBase
    {
        /// <summary>
        /// Gets the custom commands configured in a channel
        /// </summary>
        [HttpGet("{broadcasterChannelName}")]
        [ProducesResponseType(typeof(List<GetCustomCommandResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCommands([FromRoute] string broadcasterChannelName)
        {
            var commands = await apiDataService.GetCustomCommandsAsync(broadcasterChannelName);

            return commands == null ? NotFound() : Ok(commands);
        }
    }
}
