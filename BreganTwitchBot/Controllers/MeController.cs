using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeController(IMeDataService meDataService) : ControllerBase
    {
        /// <summary>
        /// The caller's stats in every channel the bot has seen them in
        /// </summary>
        [HttpGet("Stats")]
        [ProducesResponseType(typeof(GetMyStatsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyStats()
        {
            var twitchUserId = User.FindFirst("twitch_user_id")?.Value;

            if (string.IsNullOrWhiteSpace(twitchUserId))
            {
                return Unauthorized();
            }

            var stats = await meDataService.GetMyStatsAsync(twitchUserId);

            // signed in but never seen by the bot - a real state, not an error
            return Ok(stats ?? new GetMyStatsResponse
            {
                TwitchUsername = User.FindFirst("twitch_username")?.Value ?? "",
                Channels = []
            });
        }

        /// <summary>
        /// The caller's own preferences
        /// </summary>
        [HttpGet("Settings")]
        [ProducesResponseType(typeof(GetMySettingsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMySettings()
        {
            var twitchUserId = User.FindFirst("twitch_user_id")?.Value;

            if (string.IsNullOrWhiteSpace(twitchUserId))
            {
                return Unauthorized();
            }

            return Ok(await meDataService.GetMySettingsAsync(twitchUserId));
        }

        [HttpPut("Settings")]
        public async Task<IActionResult> UpdateMySetting([FromBody] UpdateMySettingRequest request)
        {
            var twitchUserId = User.FindFirst("twitch_user_id")?.Value;

            if (string.IsNullOrWhiteSpace(twitchUserId))
            {
                return Unauthorized();
            }

            try
            {
                await meDataService.UpdateMySettingAsync(twitchUserId, request);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
