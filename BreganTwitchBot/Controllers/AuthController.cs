using BreganTwitchBot.Domain.DTOs.Auth.Responses;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Cryptography;

namespace BreganTwitchBot.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IConfiguration configuration, IWebHostEnvironment environment) : ControllerBase
    {
        private const string AccessTokenCookie = "accessToken";
        private const string RefreshTokenCookie = "refreshToken";
        private const string StateCookie = "twitchOAuthState";

        /// <summary>
        /// Sends the visitor to Twitch to sign in
        /// </summary>
        [HttpGet("Login")]
        public IActionResult Login()
        {
            // the state is echoed back by twitch and checked on the callback, which
            // stops someone feeding a victim a login link for an account they control
            var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            Response.Cookies.Append(StateCookie, state, new CookieOptions
            {
                HttpOnly = true,
                Secure = UseSecureCookies,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(10)
            });

            return Redirect(authService.BuildTwitchLoginUrl(state));
        }

        /// <summary>
        /// Where Twitch sends the visitor back to
        /// </summary>
        [HttpGet("Callback")]
        public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
        {
            var websiteUrl = configuration["WebsiteUrl"] ?? "https://bot.bregan.me";

            if (!string.IsNullOrWhiteSpace(error))
            {
                Log.Information($"[Auth] Twitch login declined: {error}");
                return Redirect($"{websiteUrl}/login?error=declined");
            }

            var expectedState = Request.Cookies[StateCookie];
            Response.Cookies.Delete(StateCookie);

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state) || state != expectedState)
            {
                Log.Warning("[Auth] Twitch callback rejected - missing code or state mismatch");
                return Redirect($"{websiteUrl}/login?error=invalid");
            }

            try
            {
                var tokens = await authService.LoginWithTwitchAsync(code);
                SetAuthCookies(tokens.AccessToken, tokens.RefreshToken);

                return Redirect($"{websiteUrl}/me");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Auth] Twitch login failed");
                return Redirect($"{websiteUrl}/login?error=failed");
            }
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookie];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized();
            }

            try
            {
                var tokens = await authService.RefreshToken(refreshToken);
                SetAuthCookies(tokens.AccessToken, tokens.RefreshToken);

                return Ok();
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
            {
                ClearAuthCookies();
                return Unauthorized();
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookie];

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await authService.LogoutAsync(refreshToken);
            }

            ClearAuthCookies();
            return Ok();
        }

        /// <summary>
        /// Who the caller is
        /// </summary>
        [HttpGet("Me")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me()
        {
            var twitchUserId = User.FindFirst("twitch_user_id")?.Value;

            if (string.IsNullOrWhiteSpace(twitchUserId))
            {
                return Unauthorized();
            }

            var user = await authService.GetCurrentUserAsync(twitchUserId);

            return user == null ? NotFound() : Ok(user);
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            // httpOnly so no script on the page can read these
            Response.Cookies.Append(AccessTokenCookie, accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = UseSecureCookies,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            Response.Cookies.Append(RefreshTokenCookie, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = UseSecureCookies,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        private void ClearAuthCookies()
        {
            // the options have to line up with the ones used when the cookie was
            // written or the browser keeps the old one and every request that
            // follows still carries a token this api has already rejected
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = UseSecureCookies,
                SameSite = SameSiteMode.Lax
            };

            Response.Cookies.Delete(AccessTokenCookie, options);
            Response.Cookies.Delete(RefreshTokenCookie, options);
        }

        // the dev site is served over plain http on localhost, and a secure cookie
        // would simply be dropped there - leaving nothing to refresh with
        private bool UseSecureCookies => !environment.IsDevelopment();
    }
}
