using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Linking
{
    public class DiscordLinkingCommandModule(IDiscordLinkingData discordLinkingData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("link", "Used to link your Discord account to your Twitch account to access the Discord")]
        public async Task LinkUser([Summary("TwitchUsername", "YOUR Twitch username")] string twitchName)
        {
            var command = new DiscordCommand
            {
                UserId = Context.User.Id,
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                CommandText = twitchName
            };

            var response = await discordLinkingData.NewLinkRequest(command);
            await RespondAsync(response);
        }
    }
}
