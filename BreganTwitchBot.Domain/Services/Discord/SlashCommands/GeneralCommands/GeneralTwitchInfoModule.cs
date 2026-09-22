using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.GeneralCommands
{
    public class GeneralTwitchInfoModule(
        IDiscordGeneralCommandsData discordGeneralCommandsData,
        IDiscordUserLookupService discordUserLookupService) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("followers", "Check the follower count of the streamer")]
        public async Task TwitchFollowers()
        {
            await DeferAsync();

            var response = await discordGeneralCommandsData.GetFollowerCount(Context.Guild.Id);
            await FollowupAsync(response);
        }

        [SlashCommand("subs", "Check the sub count of the streamer")]
        public async Task SubCount()
        {
            await DeferAsync();

            var response = await discordGeneralCommandsData.GetSubscriberCount(Context.Guild.Id);
            await FollowupAsync(response);
        }

        [SlashCommand("mute", "Mute that melvin")]
        public async Task MuteUser([Summary("discorduser", "The melvin to mute")] IUser userMentioned)
        {
            await HandleMute(userMentioned, muted: true);
        }

        [SlashCommand("unmute", "Unmute that melvin")]
        public async Task UnmuteUser([Summary("discorduser", "The probably still annoying melvin to unmute")] IUser userMentioned)
        {
            await HandleMute(userMentioned, muted: false);
        }

        private async Task HandleMute(IUser userMentioned, bool muted)
        {
            // the old bot told a non mod their mute had worked while doing nothing, which
            // meant nobody could tell a real mute from a silently ignored one
            if (!discordUserLookupService.IsUserMod(Context.Guild.Id, Context.User as SocketGuildUser))
            {
                await RespondAsync("You don't have permission to do that", ephemeral: true);
                return;
            }

            await DeferAsync();

            var response = await discordGeneralCommandsData.SetUserMuted(Context.Guild.Id, userMentioned.Id, muted);
            await FollowupAsync(response);
        }
    }
}
