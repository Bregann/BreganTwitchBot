using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordUserLookupService(IConfigHelperService configHelperService, IServiceProvider serviceProvider) : IDiscordUserLookupService
    {
        public bool IsUserMod(ulong serverId, SocketGuildUser? user)
        {
            if (user == null)
            {
                return false;
            }

            var discordConfig = configHelperService.GetDiscordConfig(serverId);
            if (discordConfig != null)
            {
                var modRole = discordConfig.DiscordModeratorRoleId;
                return user.Roles.Any(role => role.Id == modRole);
            }

            return false;
        }

        //TODO: WRITE TESTS FOR THIS METHOD
        public string? GetTwitchUsernameFromDiscordUser(ulong userId)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = context.ChannelUsers.FirstOrDefault(x => x.DiscordUserId == userId);

                return user?.TwitchUsername;
            }
        }
    }
}
