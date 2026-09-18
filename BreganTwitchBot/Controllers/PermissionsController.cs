using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/Channels/{broadcasterChannelName}/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController(IPermissionService permissionService) : ControllerBase
    {
        /// <summary>
        /// Everyone with permissions in the channel. Broadcaster only.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ChannelPermissionHolderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPermissions([FromRoute] string broadcasterChannelName)
        {
            var twitchUserId = User.FindFirst("twitch_user_id")?.Value;

            if (string.IsNullOrWhiteSpace(twitchUserId))
            {
                return Unauthorized();
            }

            // who can change what is the broadcaster's business alone
            if (!await permissionService.IsBroadcasterAsync(broadcasterChannelName, twitchUserId))
            {
                return Forbid();
            }

            var permissions = await permissionService.GetPermissionsAsync(broadcasterChannelName);

            return permissions == null ? NotFound() : Ok(permissions);
        }

        /// <summary>
        /// Replaces someone's permissions in the channel. Broadcaster only.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> SetPermissions([FromRoute] string broadcasterChannelName, [FromBody] SetPermissionsRequest request)
        {
            var twitchUserId = User.FindFirst("twitch_user_id")?.Value;

            if (string.IsNullOrWhiteSpace(twitchUserId))
            {
                return Unauthorized();
            }

            try
            {
                await permissionService.SetPermissionsAsync(broadcasterChannelName, twitchUserId, request.TwitchUsername, request.Permissions);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
