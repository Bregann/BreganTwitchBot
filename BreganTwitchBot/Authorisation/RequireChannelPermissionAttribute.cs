using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BreganTwitchBot.Core.Authorisation
{
    /// <summary>
    /// Requires the caller to hold a permission in the channel named by the route.
    ///
    /// The channel comes from the route rather than the request body so a caller
    /// cannot pass a channel they do have rights in while acting on another.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireChannelPermissionAttribute(ChannelPermission permission) : Attribute, IAsyncAuthorizationFilter
    {
        private const string ChannelRouteKey = "broadcasterChannelName";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var twitchUserId = context.HttpContext.User.FindFirst("twitch_user_id")?.Value;

            if (string.IsNullOrWhiteSpace(twitchUserId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (context.RouteData.Values[ChannelRouteKey] is not string channelName)
            {
                context.Result = new BadRequestObjectResult("The channel is missing from the route");
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

            if (!await permissionService.HasPermissionAsync(channelName, twitchUserId, permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
