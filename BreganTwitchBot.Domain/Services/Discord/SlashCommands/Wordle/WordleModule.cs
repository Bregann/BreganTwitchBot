using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Wordle
{
    public class WordleModule(IDiscordWordleData discordWordleData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("wordle", "Guess today's word. Everyone gets the same one, and six tries")]
        public async Task Wordle([Summary("guess", "Your five letter guess. Leave out to see your board")] string? guess = null)
        {
            // guesses are private so nobody can copy somebody else's board
            await DeferAsync(ephemeral: true);

            if (string.IsNullOrWhiteSpace(guess))
            {
                await FollowupAsync(await discordWordleData.GetBoard(Context.Guild.Id, Context.User.Id), ephemeral: true);
                return;
            }

            var (response, publicMessage) = await discordWordleData.Guess(Context.Guild.Id, Context.User.Id, guess);
            await FollowupAsync(response, ephemeral: true);

            if (publicMessage != null)
            {
                await Context.Channel.SendMessageAsync(publicMessage);
            }
        }
    }
}
