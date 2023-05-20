using BreganTwitchBot.Domain.Data.Api;
using BreganTwitchBot.Domain.Data.Api.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SubathonController : ControllerBase
    {
        [HttpGet]
        public string GetSubathonTimeLeft()
        {
            return SubathonData.GetSubathonTimeLeft();
        }

        [HttpGet]
        public GetSubathonLeaderboardsDto GetSubathonLeaderboards()
        {
            return SubathonData.GetSubathonLeaderboards();
        }
    }
}
