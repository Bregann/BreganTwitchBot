using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.GeneralCommands
{
    public class WhoisModule(IDiscordWhoisData discordWhoisData, IDiscordUserLookupService discordUserLookupService) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("whois", "Look up a user by their Discord account or Twitch username")]
        public async Task Whois(
            [Summary("discorduser", "The Discord user to look up")] IUser? discordUser = null,
            [Summary("twitchusername", "The Twitch username to look up")] string? twitchUsername = null)
        {
            if (!discordUserLookupService.IsUserMod(Context.Guild.Id, Context.User as SocketGuildUser))
            {
                await RespondAsync("You don't have permission to do that", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            var embedData = await discordWhoisData.HandleWhoisCommandAsync(new WhoisCommand
            {
                GuildId = Context.Guild.Id,
                DiscordUserId = discordUser?.Id,
                TwitchUsername = twitchUsername
            });

            var embed = new EmbedBuilder
            {
                Timestamp = DateTime.Now,
                Color = embedData.Colour,
                Title = embedData.Title,
                Description = embedData.Description
            };

            foreach (var field in embedData.Fields)
            {
                embed.AddField(field.Key, field.Value);
            }

            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
