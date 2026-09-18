using BreganTwitchBot.Core.Authorisation;
using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/Channels/{broadcasterChannelName}/Admin")]
    [ApiController]
    [Authorize]
    public class AdminController(IAdminDataService adminDataService) : ControllerBase
    {
        // Channel config

        [HttpGet("Config")]
        [RequireChannelPermission(ChannelPermission.EditChannelConfig)]
        [ProducesResponseType(typeof(GetChannelConfigResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConfig([FromRoute] string broadcasterChannelName)
        {
            var config = await adminDataService.GetChannelConfigAsync(broadcasterChannelName);

            return config == null ? NotFound() : Ok(config);
        }

        [HttpPut("Config")]
        [RequireChannelPermission(ChannelPermission.EditChannelConfig)]
        public async Task<IActionResult> UpdateConfig([FromRoute] string broadcasterChannelName, [FromBody] UpdateChannelConfigRequest request)
        {
            return await Run(async () => await adminDataService.UpdateChannelConfigAsync(broadcasterChannelName, request));
        }

        // Custom commands

        [HttpPut("Commands")]
        [RequireChannelPermission(ChannelPermission.EditCommands)]
        public async Task<IActionResult> UpsertCommand([FromRoute] string broadcasterChannelName, [FromBody] UpsertCustomCommandRequest request)
        {
            return await Run(async () => await adminDataService.UpsertCustomCommandAsync(broadcasterChannelName, request));
        }

        [HttpDelete("Commands/{commandName}")]
        [RequireChannelPermission(ChannelPermission.EditCommands)]
        public async Task<IActionResult> DeleteCommand([FromRoute] string broadcasterChannelName, [FromRoute] string commandName)
        {
            return await Run(async () => await adminDataService.DeleteCustomCommandAsync(broadcasterChannelName, commandName));
        }

        // Ranks

        [HttpGet("Ranks")]
        [RequireChannelPermission(ChannelPermission.EditRanks)]
        [ProducesResponseType(typeof(List<GetChannelRankResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRanks([FromRoute] string broadcasterChannelName)
        {
            var ranks = await adminDataService.GetRanksAsync(broadcasterChannelName);

            return ranks == null ? NotFound() : Ok(ranks);
        }

        [HttpPut("Ranks")]
        [RequireChannelPermission(ChannelPermission.EditRanks)]
        public async Task<IActionResult> UpsertRank([FromRoute] string broadcasterChannelName, [FromBody] UpsertChannelRankRequest request)
        {
            return await Run(async () => await adminDataService.UpsertRankAsync(broadcasterChannelName, request));
        }

        [HttpDelete("Ranks/{rankId:int}")]
        [RequireChannelPermission(ChannelPermission.EditRanks)]
        public async Task<IActionResult> DeleteRank([FromRoute] string broadcasterChannelName, [FromRoute] int rankId)
        {
            return await Run(async () => await adminDataService.DeleteRankAsync(broadcasterChannelName, rankId));
        }

        // Word blacklist

        [HttpGet("Blacklist")]
        [RequireChannelPermission(ChannelPermission.EditBlacklist)]
        [ProducesResponseType(typeof(List<GetBlacklistWordResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBlacklist([FromRoute] string broadcasterChannelName)
        {
            var blacklist = await adminDataService.GetBlacklistAsync(broadcasterChannelName);

            return blacklist == null ? NotFound() : Ok(blacklist);
        }

        [HttpPost("Blacklist")]
        [RequireChannelPermission(ChannelPermission.EditBlacklist)]
        public async Task<IActionResult> AddBlacklistWord([FromRoute] string broadcasterChannelName, [FromBody] AddBlacklistWordRequest request)
        {
            return await Run(async () => await adminDataService.AddBlacklistWordAsync(broadcasterChannelName, request));
        }

        [HttpDelete("Blacklist/{wordId:int}")]
        [RequireChannelPermission(ChannelPermission.EditBlacklist)]
        public async Task<IActionResult> DeleteBlacklistWord([FromRoute] string broadcasterChannelName, [FromRoute] int wordId)
        {
            return await Run(async () => await adminDataService.DeleteBlacklistWordAsync(broadcasterChannelName, wordId));
        }

        // Discord

        [HttpGet("Discord")]
        [RequireChannelPermission(ChannelPermission.EditDiscord)]
        [ProducesResponseType(typeof(GetDiscordConfigResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDiscordConfig([FromRoute] string broadcasterChannelName)
        {
            var config = await adminDataService.GetDiscordConfigAsync(broadcasterChannelName);

            return config == null ? NotFound() : Ok(config);
        }

        [HttpPut("Discord")]
        [RequireChannelPermission(ChannelPermission.EditDiscord)]
        public async Task<IActionResult> UpdateDiscordConfig([FromRoute] string broadcasterChannelName, [FromBody] UpdateDiscordConfigRequest request)
        {
            return await Run(async () => await adminDataService.UpdateDiscordConfigAsync(broadcasterChannelName, request));
        }

        /// <summary>
        /// Turns the service's exceptions into the matching status codes, so every
        /// write endpoint answers the same way
        /// </summary>
        private async Task<IActionResult> Run(Func<Task> action)
        {
            try
            {
                await action();
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
