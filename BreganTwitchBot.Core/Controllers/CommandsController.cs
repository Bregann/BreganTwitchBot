using BreganTwitchBot.Domain.Data.Api;
using BreganTwitchBot.Domain.Data.Api.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        [HttpGet]
        public List<CustomCommandsDto> GetCommands()
        {
            return CommandsData.GetCommands();
        }
    }
}
