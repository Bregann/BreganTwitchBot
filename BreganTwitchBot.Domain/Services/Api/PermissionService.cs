using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Api;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Api
{
    /// <summary>
    /// Who may change what in a channel's admin area.
    ///
    /// Every check here runs on the server. The website hides what you cannot use,
    /// but hiding a button is not access control, so nothing relies on the UI.
    /// </summary>
    public class PermissionService(AppDbContext context) : IPermissionService
    {
        public async Task<bool> IsBroadcasterAsync(string broadcasterChannelName, string twitchUserId)
        {
            return await context.Channels.AnyAsync(x =>
                x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower() &&
                x.BroadcasterTwitchChannelId == twitchUserId);
        }

        public async Task<bool> HasPermissionAsync(string broadcasterChannelName, string twitchUserId, ChannelPermission permission)
        {
            var channel = await context.Channels
                .FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return false;
            }

            // the broadcaster can always do everything in their own channel
            if (channel.BroadcasterTwitchChannelId == twitchUserId)
            {
                return true;
            }

            return await context.ChannelPermissionGrants.AnyAsync(x =>
                x.ChannelId == channel.Id &&
                x.ChannelUser.TwitchUserId == twitchUserId &&
                x.Permission == permission);
        }

        public async Task<List<ChannelPermissionHolderResponse>?> GetPermissionsAsync(string broadcasterChannelName)
        {
            var channel = await context.Channels
                .FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                return null;
            }

            var grants = await context.ChannelPermissionGrants
                .Where(x => x.ChannelId == channel.Id)
                .Include(x => x.ChannelUser)
                .ToListAsync();

            return grants
                .GroupBy(x => x.ChannelUser.TwitchUserId)
                .Select(group => new ChannelPermissionHolderResponse
                {
                    TwitchUserId = group.Key,
                    TwitchUsername = group.First().ChannelUser.TwitchUsername,
                    Permissions = group.Select(x => x.Permission).OrderBy(x => x).ToList(),
                    GrantedAt = group.Max(x => x.GrantedAt),
                    GrantedByTwitchUserId = group.OrderByDescending(x => x.GrantedAt).First().GrantedByTwitchUserId
                })
                .OrderBy(x => x.TwitchUsername)
                .ToList();
        }

        public async Task SetPermissionsAsync(string broadcasterChannelName, string grantedByTwitchUserId, string targetTwitchUsername, List<ChannelPermission> permissions)
        {
            var channel = await context.Channels
                .FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());

            if (channel == null)
            {
                throw new KeyNotFoundException("Channel not found");
            }

            // only the broadcaster grants permissions, so a mod can neither give
            // themselves more nor take the broadcaster's away
            if (channel.BroadcasterTwitchChannelId != grantedByTwitchUserId)
            {
                throw new UnauthorizedAccessException("Only the broadcaster can manage permissions");
            }

            var targetUser = await context.ChannelUsers
                .FirstOrDefaultAsync(x => x.TwitchUsername.ToLower() == targetTwitchUsername.ToLower());

            if (targetUser == null)
            {
                throw new KeyNotFoundException($"The bot has never seen anybody called {targetTwitchUsername}");
            }

            if (targetUser.TwitchUserId == channel.BroadcasterTwitchChannelId)
            {
                throw new InvalidOperationException("The broadcaster already has every permission");
            }

            var existing = await context.ChannelPermissionGrants
                .Where(x => x.ChannelId == channel.Id && x.ChannelUserId == targetUser.Id)
                .ToListAsync();

            context.ChannelPermissionGrants.RemoveRange(existing);

            // anything that can edit implies being able to see the admin area at all,
            // otherwise a grant looks applied but the user can't reach the page
            var toGrant = permissions.Distinct().ToList();

            if (toGrant.Count > 0 && !toGrant.Contains(ChannelPermission.ViewAdmin))
            {
                toGrant.Add(ChannelPermission.ViewAdmin);
            }

            foreach (var permission in toGrant)
            {
                context.ChannelPermissionGrants.Add(new ChannelPermissionGrant
                {
                    ChannelId = channel.Id,
                    ChannelUserId = targetUser.Id,
                    Permission = permission,
                    GrantedAt = DateTime.UtcNow,
                    GrantedByTwitchUserId = grantedByTwitchUserId
                });
            }

            await context.SaveChangesAsync();

            Log.Information($"[Permissions] {grantedByTwitchUserId} set {targetUser.TwitchUsername}'s permissions in {channel.BroadcasterTwitchChannelName} to [{string.Join(", ", toGrant)}]");
        }
    }
}
