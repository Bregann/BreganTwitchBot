using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Wordle
{
    public class DiscordWordleData(AppDbContext context, IDiscordHelperService discordHelperService) : IDiscordWordleData
    {
        public async Task<WordleResponse> Guess(ulong guildId, ulong userId, string guess)
        {
            var channel = await context.GetChannelForGuild(guildId);

            if (channel == null)
            {
                return new WordleResponse("This server isn't linked to a channel", false);
            }

            guess = guess.Trim().ToLower();

            // an invalid guess isn't saved, so it doesn't cost one of the six
            if (!WordleHelper.IsWellFormedGuess(guess))
            {
                return new WordleResponse($"Your guess needs to be {WordleHelper.WordLength} letters, no numbers or spaces", true);
            }

            if (!WordleHelper.IsValidGuess(guess))
            {
                return new WordleResponse($"**{guess.ToUpper()}** isn't in the word list, try another word", true);
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var answer = WordleHelper.GetWordForDate(today);
            var game = await GetOrCreateGame(channel.Id, userId, today);

            if (IsFinished(game))
            {
                return new WordleResponse($"You've already finished today's Wordle! Come back tomorrow\n\n{BuildBoard(game, answer)}", false);
            }

            if (game.Guesses.Contains(guess))
            {
                return new WordleResponse($"You've already guessed **{guess.ToUpper()}**\n\n{BuildBoard(game, answer)}", true);
            }

            // reassigned rather than added to so EF sees the array column has changed
            game.Guesses = [.. game.Guesses, guess];
            game.Solved = guess == answer;

            await context.SaveChangesAsync();

            if (IsFinished(game))
            {
                // worked out after saving so today's game counts towards the streak
                var stats = await CalculateStats(channel.Id, userId, today);
                var result = $"Unlucky! The word was **{answer.ToUpper()}**";

                if (game.Solved)
                {
                    var basePoints = WordleHelper.GetPointsForGuesses(game.Guesses.Count);
                    var streakBonus = WordleHelper.GetStreakBonus(stats.CurrentStreak);

                    game.PointsAwarded = await AwardPoints(guildId, userId, basePoints + streakBonus);
                    await context.SaveChangesAsync();

                    var streakText = streakBonus > 0 ? $" (**{basePoints:N0}** + **{streakBonus:N0}** for your {stats.CurrentStreak} day streak)" : "";

                    result = $"You got it in **{game.Guesses.Count}/{WordleHelper.MaxGuesses}**!" + (game.PointsAwarded > 0
                        ? $" You've earned **{game.PointsAwarded:N0}** points{streakText}!"
                        : " Link your Twitch account to earn points for solving it.");
                }

                return new WordleResponse($"{result}\n\n{BuildBoard(game, answer)}\n\n{WordleHelper.RenderStats(stats)}", false,
                    BuildPublicMessage(userId, game, answer, stats));
            }

            return new WordleResponse($"{BuildBoard(game, answer)}\n\n{WordleHelper.MaxGuesses - game.Guesses.Count} guesses left", true);
        }

        public async Task<WordleResponse> GetBoard(ulong guildId, ulong userId)
        {
            var channel = await context.GetChannelForGuild(guildId);

            if (channel == null)
            {
                return new WordleResponse("This server isn't linked to a channel", false);
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var game = await context.DiscordWordleGames.FirstOrDefaultAsync(x => x.ChannelId == channel.Id && x.DiscordUserId == userId && x.Date == today);

            if (game == null || game.Guesses.Count == 0)
            {
                return new WordleResponse($"You haven't guessed yet today. Press the button to guess a {WordleHelper.WordLength} letter word, you get {WordleHelper.MaxGuesses} tries", true);
            }

            var answer = WordleHelper.GetWordForDate(today);
            var board = BuildBoard(game, answer);

            return IsFinished(game)
                ? new WordleResponse($"You've finished today's Wordle! Come back tomorrow\n\n{board}", false)
                : new WordleResponse($"{board}\n\n{WordleHelper.MaxGuesses - game.Guesses.Count} guesses left", true);
        }

        public async Task<WordleStats?> GetStats(ulong guildId, ulong userId)
        {
            var channel = await context.GetChannelForGuild(guildId);

            if (channel == null)
            {
                return null;
            }

            return await CalculateStats(channel.Id, userId, DateOnly.FromDateTime(DateTime.UtcNow));
        }

        private async Task<WordleStats> CalculateStats(int channelId, ulong userId, DateOnly today)
        {
            // one row per day played, so this stays small even for a regular player
            var games = await context.DiscordWordleGames
                .Where(x => x.ChannelId == channelId && x.DiscordUserId == userId)
                .Select(x => new WordleGameSummary(x.Date, x.Guesses.Count, x.Solved))
                .ToListAsync();

            return WordleHelper.CalculateStats(games, today);
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
        private async Task<long> AwardPoints(ulong guildId, ulong userId, long points)
        {
            var isLinked = await context.ChannelUsers.AnyAsync(x => x.DiscordUserId == userId);

            if (!isLinked)
            {
                return 0;
            }

            await discordHelperService.AddPointsToUser(guildId, userId, points);

            Log.Information($"[Discord Wordle] {userId} solved it and earned {points} points");
            return points;
        }

        private static bool IsFinished(DiscordWordleGame game)
        {
            return game.Solved || game.Guesses.Count >= WordleHelper.MaxGuesses;
        }

        private static string BuildBoard(DiscordWordleGame game, string answer)
        {
            var board = WordleHelper.RenderBoard(game.Guesses, answer, showLetters: true);

            // the keyboard is only useful while there are guesses left to make
            return game.Guesses.Count == 0 || IsFinished(game) ? board : $"{board}\n{WordleHelper.RenderKeyboard(game.Guesses, answer)}";
        }

        /// <summary>
        /// Squares only, so it can be posted in the channel without giving the word away
        /// </summary>
        private static string BuildPublicMessage(ulong userId, DiscordWordleGame game, string answer, WordleStats stats)
        {
            var headline = game.Solved
                ? $"<@{userId}> solved today's Wordle in **{game.Guesses.Count}/{WordleHelper.MaxGuesses}**!"
                : $"<@{userId}> missed today's Wordle **X/{WordleHelper.MaxGuesses}**";

            var streak = game.Solved
                ? $"🔥 {stats.CurrentStreak} day streak"
                : stats.MaxStreak > 0 ? $"Streak reset (best is {stats.MaxStreak})" : "";

            var earned = game.PointsAwarded > 0 ? $" · earned {game.PointsAwarded:N0} points" : "";
            var summary = $"{streak}{(streak == "" ? "" : " · ")}{stats.WinPercentage}% won from {stats.Played} played{earned}";

            return $"{headline}\n{WordleHelper.RenderBoard(game.Guesses, answer, showLetters: false)}\n{summary}";
        }
    }
}
