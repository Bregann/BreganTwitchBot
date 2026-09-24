using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Wordle
{
    /// <summary>
    /// Guesses are typed into a pop up rather than passed as a slash command option, so the word
    /// is never shown as "used /wordle guess: crane" - not in the channel, and not on the
    /// player's own screen if they're streaming or screenshotting it.
    /// The button and pop up are handled in DiscordService, as buttons are dispatched there.
    /// </summary>
    public class WordleModule(IDiscordWordleData discordWordleData) : InteractionModuleBase<SocketInteractionContext>
    {
        public const string GuessButtonId = "wordle-guess";
        public const string GuessModalId = "wordle-modal";
        public const string GuessInputId = "wordle-guess-input";

        [SlashCommand("wordle", "Guess today's word. Everyone gets the same one, and six tries")]
        public async Task Wordle()
        {
            // the board is private so nobody can copy somebody else's guesses
            await DeferAsync(ephemeral: true);

            var board = await discordWordleData.GetBoard(Context.Guild.Id, Context.User.Id);
            await FollowupAsync(board.Response, ephemeral: true, components: BuildGuessButton(board.CanGuess));
        }

        [SlashCommand("wordlestats", "See Wordle stats for you or someone else")]
        public async Task WordleStats([Summary("user", "Whose stats to see. Leave out for your own")] IUser? user = null)
        {
            await DeferAsync();

            user ??= Context.User;
            var stats = await discordWordleData.GetStats(Context.Guild.Id, user.Id);

            if (stats == null)
            {
                await FollowupAsync("This server isn't linked to a channel");
                return;
            }

            if (stats.Played == 0)
            {
                await FollowupAsync($"{user.Username} hasn't played Wordle yet! Use `/wordle` to start");
                return;
            }

            await FollowupAsync($"**Wordle stats for {user.Username}**\n{WordleHelper.RenderStats(stats)}", allowedMentions: AllowedMentions.None);
        }

        /// <summary>
        /// The button that opens the guess pop up, or nothing once the game is over
        /// </summary>
        public static MessageComponent? BuildGuessButton(bool canGuess)
        {
            return canGuess
                ? new ComponentBuilder().WithButton("Guess", GuessButtonId, ButtonStyle.Primary, new Emoji("🔤")).Build()
                : null;
        }

        public static Modal BuildGuessModal()
        {
            return new ModalBuilder()
                .WithTitle("Wordle")
                .WithCustomId(GuessModalId)
                .AddTextInput("Your guess", GuessInputId, TextInputStyle.Short, "five letters", WordleHelper.WordLength, WordleHelper.WordLength, required: true)
                .Build();
        }
    }
}
