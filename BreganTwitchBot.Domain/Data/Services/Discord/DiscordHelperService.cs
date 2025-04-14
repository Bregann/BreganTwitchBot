using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordHelperService(IDiscordService discordService, IConfigHelperService configHelperService, IServiceProvider serviceProvider) : IDiscordHelperService
    {
        public async Task SendMessage(ulong channelId, string message)
        {
            try
            {
                var channel = discordService.Client.GetChannel(channelId) as IMessageChannel;

                if (channel != null)
                {
                    await channel.TriggerTypingAsync();
                    await channel.SendMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"[Discord Message Send] Failed to send message - {e}");
            }
        }

        public async Task SendEmbedMessage(ulong channelId, EmbedBuilder embed)
        {
            try
            {
                var channel = discordService.Client.GetChannel(channelId) as IMessageChannel;

                if (channel != null)
                {
                    await channel.TriggerTypingAsync();
                    await channel.SendMessageAsync(embed: embed.Build());
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"[Discord Message Send] Failed to send message - {ex}");
            }
        }

        public bool IsUserMod(ulong serverId, ulong userId)
        {
            var user = discordService.Client.GetGuild(serverId).GetUser(userId);

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

        public string? GetTwitchUsernameFromDiscordUser(ulong userId)
        {
            using(var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = context.ChannelUsers.FirstOrDefault(x => x.DiscordUserId == userId);

                return user?.TwitchUsername;
            }
        }
    }
}
