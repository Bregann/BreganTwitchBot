using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.SelfAssignRoles
{
    public class SelfAssignRolesModule(IDiscordSelfAssignRoleData discordSelfAssignRoleData, IDiscordUserLookupService discordUserLookupService) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("roles", "Post the self assign roles panel")]
        public async Task PostRolesPanel()
        {
            if (!discordUserLookupService.IsUserMod(Context.Guild.Id, Context.User as SocketGuildUser))
            {
                await RespondAsync("You don't have permission to do that", ephemeral: true);
                return;
            }

            var roles = await discordSelfAssignRoleData.GetRolesAsync(Context.Guild.Id);

            if (roles.Count == 0)
            {
                await RespondAsync("There aren't any self assign roles configured for this server", ephemeral: true);
                return;
            }

            var builder = new ComponentBuilder();

            // discord allows 5 rows of 5 buttons
            foreach (var (role, index) in roles.Take(25).Select((role, index) => (role, index)))
            {
                builder.WithButton(
                    role.DisplayName,
                    $"selfrole-{role.Id}",
                    ButtonStyle.Secondary,
                    string.IsNullOrWhiteSpace(role.Emoji) ? null : new Emoji(role.Emoji),
                    row: index / 5);
            }

            await RespondAsync("Click a button to give yourself a role, or click it again to remove it", components: builder.Build());
        }
    }
}
