using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.Giveaway
{
    public class GiveawayModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("startgiveaway", "Start a new giveaway")]
        public async Task StartNewGiveaway()
        {
            if (Context.Channel.Id != AppConfig.DiscordGiveawayChannelID)
            {
                await RespondAsync("Your giveaway has been unstarted!", ephemeral: true);
                return;
            }

            var builder = new ComponentBuilder()
                .WithButton("Enter giveaway", $"{Context.Interaction.Id}-enter", ButtonStyle.Success, new Emoji("🎉"))
                .WithButton("Check entries", $"{Context.Interaction.Id}-check", ButtonStyle.Primary, new Emoji("❓"));

            await RespondAsync("A new giveaway has started! Click the button to enter the giveaway", components: builder.Build());
        }
    }
}
