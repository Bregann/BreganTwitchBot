using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Gambling;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.Gambling
{
    public class DiscordGamblingModule : InteractionModuleBase<SocketInteractionContext>
    {
        public static Dictionary<ulong, DateTime> SpinCooldownDict = new Dictionary<ulong, DateTime>();

        [SlashCommand("spin", "Spin your points")]
        public async Task SpinPoints([Summary("Points", "The amount of points you are gambling (type all to spin them all!)")] string pointsBeingGambled)
        {
            await DeferAsync();
            EmbedBuilder embed;

            if (!SpinCooldownDict.ContainsKey(Context.User.Id))
            {
                SpinCooldownDict.Add(Context.User.Id, DateTime.UtcNow);
                embed = await GamblingData.HandleSpinCommand(pointsBeingGambled.ToLower(), Context);
                await FollowupAsync(embed: embed.Build());
                return;
            }

            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= SpinCooldownDict[Context.User.Id]) //another anti davno code
            {
                embed = new EmbedBuilder();
                embed.WithTitle("You are currently on cooldown");
                await FollowupAsync(embed: embed.Build());
                return;
            }

            SpinCooldownDict[Context.User.Id] = DateTime.UtcNow;
            embed = await GamblingData.HandleSpinCommand(pointsBeingGambled.ToLower(), Context);
            await FollowupAsync(embed: embed.Build());
        }
    }
}