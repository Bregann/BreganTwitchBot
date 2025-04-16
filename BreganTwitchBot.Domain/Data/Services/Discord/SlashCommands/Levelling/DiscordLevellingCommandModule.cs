using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Levelling
{
    public class LevellingCommandModule(IDiscordLevellingData discordLevellingData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("togglelevelups", "Disable or enable level ups")]
        public async Task ToggleLevelUpNotifs()
        {
            var command = new DiscordCommand
            {
                UserId = Context.User.Id,
                GuildId = Context.Guild.Id,
            };

            var response = await discordLevellingData.HandleToggleLevelUpCommand(command);
            await RespondAsync(response);
        }
    }
}
