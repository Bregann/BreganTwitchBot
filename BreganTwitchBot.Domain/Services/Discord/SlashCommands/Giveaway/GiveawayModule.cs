using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Giveaway
{
    public class GiveawayModule(IDiscordGiveawayData discordGiveawayData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("startgiveaway", "Start a new giveaway")]
        public async Task StartNewGiveaway(
            [Summary("minimumhours", "Hours of watchtime needed to enter. Leave out for no minimum")] int minimumWatchtimeHours = 0,
            [Summary("requiredrank", "The rank needed to enter. Leave out for no rank requirement")] string? requiredRank = null)
        {
            var (giveawayId, response) = await discordGiveawayData.StartGiveawayAsync(Context.Guild.Id, Context.Channel.Id, Context.User.Id, minimumWatchtimeHours, requiredRank);

            if (giveawayId == null)
            {
                await RespondAsync(response, ephemeral: true);
                return;
            }

            var builder = new ComponentBuilder()
                .WithButton("Enter giveaway", $"giveaway-{giveawayId}-enter", ButtonStyle.Success, new Emoji("🎉"))
                .WithButton("Check entries", $"giveaway-{giveawayId}-check", ButtonStyle.Primary, new Emoji("❓"))
                .WithButton("Draw winner", $"giveaway-{giveawayId}-draw", ButtonStyle.Danger, new Emoji("🏆"));

            await RespondAsync(response, components: builder.Build());
        }
    }
}
