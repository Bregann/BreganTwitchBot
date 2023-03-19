using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Linking;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.Linking
{
    public class LinkingModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("link", "Used to link your Discord account to your Twitch account to access the Discord")]
        public async Task LinkUser([Summary("TwitchUsername", "YOUR Twitch username")] string twitchName)
        {
            await DeferAsync();
            if (Context.Channel.Id != AppConfig.DiscordLinkingChannelID)
            {
                await RespondAsync("you silly sausage you can't be using commands here!", ephemeral: true);
                return;
            }

            var response = await DiscordLinking.NewLinkRequest(twitchName.ToLower(), Context.User.Id);
            await FollowupAsync(response);
        }

        [SlashCommand("mlink", "Used for mots to manually link users")]
        public async Task ManualLinkUser([Summary("TwitchUsername", "Username of geezer you are linking")] string twitchName, [Summary("DiscordUser", "@ the discord geezer you're linking")] IUser discordUser)
        {
            var isMod = DiscordHelper.IsUserMod(Context.User.Id);

            if (!isMod)
            {
                await RespondAsync("You aren't a mot lol", ephemeral: true);
                return;
            }

            var response = await DiscordLinking.ManualLinkRequest(discordUser.Id, twitchName.ToLower());
            await RespondAsync(response);
        }
    }
}