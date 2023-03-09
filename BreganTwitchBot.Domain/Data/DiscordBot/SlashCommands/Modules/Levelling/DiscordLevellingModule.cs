using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Levelling;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.Levelling
{
    public class LevellingCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("level", "Check your or another users Discord level")]
        public async Task CheckLevel([Summary("DiscordUser", "@ the discord user")] IUser discordUser = null)
        {
            await DeferAsync();
            await Task.Run(async () => await DiscordLevelling.HandleLevelCommand(Context, discordUser));
        }

        [SlashCommand("togglelevelups", "Disable or enable level ups")]
        public async Task ToggleLevelUpNotifs()
        {
            await DiscordLevelling.HandleToggleLevelUpCommand(Context);
        }
    }
}