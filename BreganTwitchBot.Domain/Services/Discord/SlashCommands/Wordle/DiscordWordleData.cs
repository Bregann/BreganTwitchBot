using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Wordle
{
    public class DiscordWordleData(AppDbContext context, IDiscordHelperService discordHelperService) : IDiscordWordleData
    {
        public async Task<(string Response, string? PublicMessage)> Guess(ulong guildId, ulong userId, string guess)
        {
            var channel = await context.GetChannelForGuild(guildId);

            if (channel == null)
            {
                return ("This server isn't linked to a channel", null);
            }

            guess = guess.Trim().ToLower();

            // an invalid guess isn't saved, so it doesn't cost one of the six
            if (!WordleHelper.IsValidGuess(guess))
            {
                return ($"Your guess needs to be {WordleHelper.WordLength} letters, no numbers or spaces", null);
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var answer = WordleHelper.GetWordForDate(today);
            var game = await GetOrCreateGame(channel.Id, userId, today);

            if (IsFinished(game))
            {
                return ($"You've already finished today's Wordle! Come back tomorrow\n\n{BuildBoard(game, answer)}", null);
            }

            if (game.Guesses.Contains(guess))
            {
                return ($"You've already guessed **{guess.ToUpper()}**\n\n{BuildBoard(game, answer)}", null);
            }

            // reassigned rather than added to so EF sees the array column has changed
            game.Guesses = [.. game.Guesses, guess];
            game.Solved = guess == answer;

            if (game.Solved)
            {
                game.PointsAwarded = await AwardPoints(guildId, userId, game.Guesses.Count);
            }

            await context.SaveChangesAsync();

            if (game.Solved)
            {
                var pointsText = game.PointsAwarded > 0
                    ? $" You've earned **{game.PointsAwarded:N0}** points!"
                    : " Link your Twitch account to earn points for solving it.";

                return ($"You got it in **{game.Guesses.Count}/{WordleHelper.MaxGuesses}**!{pointsText}\n\n{BuildBoard(game, answer)}",
                    BuildPublicMessage(userId, game, answer));
            }

            if (IsFinished(game))
            {
                return ($"Unlucky! The word was **{answer.ToUpper()}**\n\n{BuildBoard(game, answer)}",
                    BuildPublicMessage(userId, game, answer));
            }

            return ($"{BuildBoard(game, answer)}\n\n{WordleHelper.MaxGuesses - game.Guesses.Count} guesses left", null);
        }

        public async Task<string> GetBoard(ulong guildId, ulong userId)
        {
            var channel = await context.GetChannelForGuild(guildId);

            if (channel == null)
            {
                return "This server isn't linked to a channel";
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var game = await context.DiscordWordleGames.FirstOrDefaultAsync(x => x.ChannelId == channel.Id && x.DiscordUserId == userId && x.Date == today);

            if (game == null || game.Guesses.Count == 0)
            {
                return $"You haven't guessed yet today. Use `/wordle guess:` with a {WordleHelper.WordLength} letter word, you get {WordleHelper.MaxGuesses} tries";
            }

            return BuildBoard(game, WordleHelper.GetWordForDate(today));
        }

        private async Task<DiscordWordleGame> GetOrCreateGame(int channelId, ulong userId, DateOnly date)
        {
            var game = await context.DiscordWordleGames.FirstOrDefaultAsync(x => x.ChannelId == channelId && x.DiscordUserId == userId && x.Date == date);

            if (game != null)
            {
                return game;
            }

            game = new DiscordWordleGame
            {
                ChannelId = channelId,
                DiscordUserId = userId,
                Date = date,
                Guesses = [],
                Solved = false,
                PointsAwarded = 0
            };

            await context.DiscordWordleGames.AddAsync(game);
            return game;
        }

        /// <summary>
        /// Only linked users have a points balance to pay into
        /// </summary>
        private async Task<long> AwardPoints(ulong guildId, ulong userId, int guessesUsed)
        {
            var isLinked = await context.ChannelUsers.AnyAsync(x => x.DiscordUserId == userId);

            if (!isLinked)
            {
                return 0;
            }

            var points = WordleHelper.GetPointsForGuesses(guessesUsed);
            await discordHelperService.AddPointsToUser(guildId, userId, points);

            Log.Information($"[Discord Wordle] {userId} solved in {guessesUsed} and earned {points} points");
            return points;
        }

        private static bool IsFinished(DiscordWordleGame game)
        {
            return game.Solved || game.Guesses.Count >= WordleHelper.MaxGuesses;
        }

        private static string BuildBoard(DiscordWordleGame game, string answer)
        {
            var board = WordleHelper.RenderBoard(game.Guesses, answer, showLetters: true);
            var ruledOut = WordleHelper.GetRuledOutLetters(game.Guesses, answer);

            return string.IsNullOrEmpty(ruledOut) || IsFinished(game) ? board : $"{board}\n\nNot in the word: {ruledOut}";
        }

        /// <summary>
        /// Squares only, so it can be posted in the channel without giving the word away
        /// </summary>
        private static string BuildPublicMessage(ulong userId, DiscordWordleGame game, string answer)
        {
            var score = game.Solved ? $"{game.Guesses.Count}/{WordleHelper.MaxGuesses}" : $"X/{WordleHelper.MaxGuesses}";
            return $"<@{userId}> played today's Wordle **{score}**\n{WordleHelper.RenderBoard(game.Guesses, answer, showLetters: false)}";
        }
    }
}
