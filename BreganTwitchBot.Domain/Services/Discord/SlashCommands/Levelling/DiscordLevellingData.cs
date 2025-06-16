using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Levelling
{
    //TODO: WRITE TESTS FOR THIS class
    public class DiscordLevellingData(AppDbContext context) : IDiscordLevellingData
    {
        public async Task<string> HandleToggleLevelUpCommand(DiscordCommand command)
        {
            var channel = await context.Channels.FirstAsync(x => x.ChannelConfig.DiscordGuildId == command.GuildId);
            var user = await context.DiscordUserStats.FirstAsync(x => x.User.DiscordUserId == command.UserId && x.ChannelId == channel.Id);

            // change level ups toggle and return the enabled or disabled message
            user.DiscordLevelUpNotifsEnabled = !user.DiscordLevelUpNotifsEnabled;
            await context.SaveChangesAsync();

            return user.DiscordLevelUpNotifsEnabled ? "Level up notifications have been enabled!" : "Level up notifications have been disabled!";
        }
    }
}
