using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord.Interactions;

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
                ChannelId = Context.Channel.Id
            };

            var response = await discordLevellingData.HandleToggleLevelUpCommand(command);
            await RespondAsync(response);
        }
    }
}
