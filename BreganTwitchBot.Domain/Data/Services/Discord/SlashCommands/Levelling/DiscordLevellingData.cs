using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Levelling
{
    //TODO: WRITE TESTS FOR THIS class
    public class DiscordLevellingData(AppDbContext context) : IDiscordLevellingData
    {
        public async Task<string> HandleToggleLevelUpCommand(DiscordCommand command)
        {
            var user = await context.DiscordUserStats.FirstAsync(x => x.User.DiscordUserId == command.UserId && x.Channel.DiscordGuildId == command.GuildId);

            // change level ups toggle and return the enabled or disabled message
            user.DiscordLevelUpNotifsEnabled = !user.DiscordLevelUpNotifsEnabled;
            await context.SaveChangesAsync();

            return user.DiscordLevelUpNotifsEnabled ? "Level up notifications have been enabled!" : "Level up notifications have been disabled!";
        }
    }
}
