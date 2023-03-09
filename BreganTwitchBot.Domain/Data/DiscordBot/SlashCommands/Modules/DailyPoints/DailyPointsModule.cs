using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.GeneralCommands.DailyPoints;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.DailyPoints
{
    public class DailyPointsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("daily", "Claim your daily points")]
        public async Task DailyPointsCollection()
        {
            await DeferAsync();
            var embedResponse = await DailyData.HandleDailyCommand(Context);
            await FollowupAsync(embed: embedResponse.Build());
        }
    }
}