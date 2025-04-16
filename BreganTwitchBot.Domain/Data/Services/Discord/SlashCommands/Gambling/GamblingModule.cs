using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Gambling
{
    public class DailyPointsModule(IDiscordGamblingData discordGamblingDataService) : InteractionModuleBase<SocketInteractionContext>
    {
        private static Dictionary<ulong, DateTime> _spinCooldownDict = new Dictionary<ulong, DateTime>();

        [SlashCommand("spin", "Spin your points")]
        public async Task SpinPoints([Summary("Points", "The amount of points you are gambling (type all to spin them all!)")] string pointsBeingGambled)
        {
            await DeferAsync();

            if (!_spinCooldownDict.ContainsKey(Context.User.Id))
            {
                _spinCooldownDict.Add(Context.User.Id, DateTime.Now);
            }
            else
            {
                var timeSinceLastSpin = DateTime.Now - _spinCooldownDict[Context.User.Id];
                if (timeSinceLastSpin.TotalSeconds < 5)
                {
                    await FollowupAsync($"You need to wait {5 - (int)timeSinceLastSpin.TotalSeconds} seconds before spinning again.");
                    return;
                }
                _spinCooldownDict[Context.User.Id] = DateTime.Now;
            }

            var command = new DiscordCommand
            {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id,
                CommandText = pointsBeingGambled
            };

            var embedData = await discordGamblingDataService.HandleSpinCommand(command);

            var embed = new EmbedBuilder
            {
                Timestamp = DateTime.Now,
                Color = embedData.Colour,
                Title = embedData.Title,
                Description = embedData.Description,
            };

            foreach (var field in embedData.Fields)
            {
                embed.AddField(field.Key, field.Value);
            }

            embed.WithAuthor($"Spin - {Context.User.Username}", null, Context.User.GetAvatarUrl());
            await FollowupAsync(embed: embed.Build());
        }
    }
}
