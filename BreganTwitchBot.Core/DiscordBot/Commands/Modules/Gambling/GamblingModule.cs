using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.Gambling
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
                SpinCooldownDict.Add(Context.User.Id, DateTime.Now);
                embed = await Gambling.HandleSpinCommand(pointsBeingGambled.ToLower(), Context);
                await FollowupAsync(embed: embed.Build());
                return;
            }

            if (DateTime.Now - TimeSpan.FromSeconds(5) <= SpinCooldownDict[Context.User.Id]) //another anti davno code
            {
                return;
            }

            SpinCooldownDict[Context.User.Id] = DateTime.Now;
            embed = await Gambling.HandleSpinCommand(pointsBeingGambled.ToLower(), Context);
            await FollowupAsync(embed: embed.Build());
        }
    }
}
