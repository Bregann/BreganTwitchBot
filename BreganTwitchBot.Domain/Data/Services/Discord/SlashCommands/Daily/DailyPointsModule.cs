using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Daily
{
    public class DailyPointsModule(IDiscordDailyPointsData discordDailyPointsData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("daily", "Claim your daily points")]
        public async Task DailyPointsCollection()
        {
            await DeferAsync();

            var command = new DiscordCommand
            {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id,
                ChannelId = Context.Channel.Id,
            };

            var embedData = await discordDailyPointsData.HandleDiscordDailyPointsCommand(command);

            var embed = new EmbedBuilder()
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

            embed.WithAuthor($"Daily Points - {Context.User.Username}", null, Context.User.GetAvatarUrl());

            await FollowupAsync(embed: embed.Build());
        }
    }
}
