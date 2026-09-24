using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController(ICommandHandler commandHandler) : ControllerBase
    {
        /// <summary>
        /// Every built in command the bot responds to, read from the live registry
        /// </summary>
        [HttpGet("Commands")]
        [ProducesResponseType(typeof(List<GetPublicCommandResponse>), StatusCodes.Status200OK)]
        public IActionResult GetCommands()
        {
            var commands = commandHandler.GetRegisteredCommands()
                .Select(x => new GetPublicCommandResponse
                {
                    CommandName = x.CommandName,
                    Aliases = x.Aliases
                })
                .ToList();

            return Ok(commands);
        }
    }
}
