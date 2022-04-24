using Discord.Interactions;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.GeneralCommands.DailyPoints
{
    public class DailyPointsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("daily", "Claim your daily points")]
        public async Task DailyPointsCollection()
        {
            await DeferAsync();
            var embedResponse = await Daily.HandleDailyCommand(Context);
            await FollowupAsync(embed: embedResponse.Build());
        }
    }
}
