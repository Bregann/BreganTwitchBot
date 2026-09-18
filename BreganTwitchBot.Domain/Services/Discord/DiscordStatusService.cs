using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Discord;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord
{
    public class DiscordStatusService(AppDbContext context, IDiscordClientProvider discordClientProvider) : IDiscordStatusService
    {
        public async Task UpdateMemberCountStatusAsync()
        {
            try
            {
                var guildIds = await context.Channels
                    .Where(x => x.ChannelConfig.DiscordGuildId != null)
                    .Select(x => x.ChannelConfig.DiscordGuildId!.Value)
                    .ToListAsync();

                if (guildIds.Count == 0)
                {
                    return;
                }

                // the bot has one presence but may now serve several guilds, so the status is the
                // total across all of them rather than one guild's count
                var totalMembers = 0;

                foreach (var guildId in guildIds)
                {
                    var guild = discordClientProvider.Client.GetGuild(guildId);

                    if (guild != null)
                    {
                        totalMembers += guild.MemberCount;
                    }
                }

                await discordClientProvider.Client.SetGameAsync($"{totalMembers:N0} members", null, ActivityType.Watching);
                Log.Information($"[Discord Status] Member count status updated to {totalMembers} members");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Discord Status] Error updating the member count status");
            }
        }
    }
}
