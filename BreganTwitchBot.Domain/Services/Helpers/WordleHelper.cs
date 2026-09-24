using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using System.Text;

namespace BreganTwitchBot.Domain.Services.Helpers
{
    public enum WordleLetterResult
    {
        Absent,
        Present,
        Correct
    }

    public static class WordleHelper
    {
        public const int WordLength = 5;
        public const int MaxGuesses = 6;

        /// <summary>
        /// The words that can be the answer: the most common everyday words from Wordle's
        /// guess list, without plurals, past tenses, names, slang or anything unpleasant
        /// </summary>
        public static readonly IReadOnlyList<string> Answers = LoadWords("wordle-answers.txt");

        /// <summary>
        /// Every word Wordle accepts as a guess, so a guess has to be a real word
        /// </summary>
        public static readonly IReadOnlySet<string> ValidGuesses = LoadWords("wordle-guesses.txt").ToHashSet();

        /// <summary>
        /// Extra points for each day of a streak after the first, up to the cap
        /// </summary>
        public const long StreakBonusPerDay = 500;
        public const long MaxStreakBonus = 10_000;

        /// <summary>
        /// Points for solving in 1, 2, 3... guesses. Failing earns nothing.
        /// </summary>
        private static readonly long[] PointsByGuesses = [5000, 3000, 2000, 1500, 1000, 500];

        /// <summary>
        /// The word for a day. Stepping through the list by a prime means every word comes up
        /// once before any repeats, without needing to store which have been used.
        /// </summary>
        public static string GetWordForDate(DateOnly date)
        {
            var index = (int)((long)date.DayNumber * 7919 % Answers.Count);
            return Answers[index];
        }

        /// <summary>
        /// Five lowercase letters, before checking it's actually a word
        /// </summary>
        public static bool IsWellFormedGuess(string guess)
        {
            return guess.Length == WordLength && guess.All(char.IsAsciiLetterLower);
        }

        public static bool IsValidGuess(string guess)
        {
            return IsWellFormedGuess(guess) && ValidGuesses.Contains(guess);
        }

        /// <summary>
        /// Scores a guess the way Wordle does: exact matches first, then a letter elsewhere in
        /// the word is only marked present as many times as it's still unaccounted for. So
        /// guessing "geese" against "eagle" doesn't light up all three e's.
        /// </summary>
        public static WordleLetterResult[] Score(string guess, string answer)
        {
            var results = new WordleLetterResult[WordLength];
            var remaining = new Dictionary<char, int>();

            for (var i = 0; i < WordLength; i++)
            {
                if (guess[i] == answer[i])
                {
                    results[i] = WordleLetterResult.Correct;
                }
                else
                {
                    remaining[answer[i]] = remaining.GetValueOrDefault(answer[i]) + 1;
                }
            }

            for (var i = 0; i < WordLength; i++)
            {
                if (results[i] == WordleLetterResult.Correct)
                {
                    continue;
                }

                if (remaining.GetValueOrDefault(guess[i]) > 0)
                {
                    results[i] = WordleLetterResult.Present;
                    remaining[guess[i]]--;
                }
            }

            return results;
        }

        public static long GetPointsForGuesses(int guessesUsed)
        {
            return guessesUsed >= 1 && guessesUsed <= PointsByGuesses.Length ? PointsByGuesses[guessesUsed - 1] : 0;
        }

        /// <summary>
        /// The bonus for a win that makes the streak this long. A first win has no streak to reward yet.
        /// </summary>
        public static long GetStreakBonus(int streak)
        {
            return Math.Min(Math.Max(streak - 1, 0) * StreakBonusPerDay, MaxStreakBonus);
        }

        private static string[] LoadWords(string resourceName)
        {
            using var stream = typeof(WordleHelper).Assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"The Wordle word list {resourceName} isn't embedded in the assembly");
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd()
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        /// <summary>
        /// The coloured squares for a guess, with the letters after them when they can be shown
        /// </summary>
        public static string RenderRow(string guess, string answer, bool showLetters)
        {
            var squares = string.Concat(Score(guess, answer).Select(x => x switch
            {
                WordleLetterResult.Correct => "🟩",
                WordleLetterResult.Present => "🟨",
                _ => "⬛"
            }));

            return showLetters ? $"{squares} `{guess.ToUpper()}`" : squares;
        }

        private static readonly string[] KeyboardRows = ["qwertyuiop", "asdfghjkl", "zxcvbnm"];

        private const string AnsiReset = "\u001b[0m";
        private const string AnsiGreen = "\u001b[1;32m";
        private const string AnsiYellow = "\u001b[1;33m";
        private const string AnsiGrey = "\u001b[0;30m";

        /// <summary>
        /// What's known about each guessed letter. A letter keeps its best result, so a repeated
        /// letter scored absent in one spot doesn't hide that it's present or correct in another.
        /// Letters not in the dictionary haven't been guessed yet.
        /// </summary>
        public static Dictionary<char, WordleLetterResult> GetLetterStates(IEnumerable<string> guesses, string answer)
        {
            var states = new Dictionary<char, WordleLetterResult>();

            foreach (var guess in guesses)
            {
                var results = Score(guess, answer);

                for (var i = 0; i < WordLength; i++)
                {
                    if (!states.TryGetValue(guess[i], out var known) || results[i] > known)
                    {
                        states[guess[i]] = results[i];
                    }
                }
            }

            return states;
        }

        /// <summary>
        /// A QWERTY keyboard like Wordle's, coloured with Discord's ansi code blocks: green in the
        /// right place, yellow in the word, and letters that aren't in the word swapped for a dot.
        /// Clients that can't show the colours still get the dots, so the letters left to try
        /// are always clear.
        /// </summary>
        public static string RenderKeyboard(IEnumerable<string> guesses, string answer)
        {
            var states = GetLetterStates(guesses, answer);
            var keyboard = new StringBuilder("```ansi\n");

            for (var row = 0; row < KeyboardRows.Length; row++)
            {
                // staggered like a real keyboard
                keyboard.Append(new string(' ', row * 2));

                var keys = KeyboardRows[row].Select(letter =>
                {
                    var key = char.ToUpper(letter);

                    if (!states.TryGetValue(letter, out var state))
                    {
                        return key.ToString();
                    }

                    return state switch
                    {
                        WordleLetterResult.Correct => $"{AnsiGreen}{key}{AnsiReset}",
                        WordleLetterResult.Present => $"{AnsiYellow}{key}{AnsiReset}",
                        _ => $"{AnsiGrey}·{AnsiReset}"
                    };
                });

                keyboard.AppendLine(string.Join(" ", keys));
            }

            return keyboard.Append("```").ToString();
        }

        public static string RenderBoard(IReadOnlyList<string> guesses, string answer, bool showLetters)
        {
            var board = new StringBuilder();

            foreach (var guess in guesses)
            {
                board.AppendLine(RenderRow(guess, answer, showLetters));
            }

            return board.ToString().TrimEnd();
        }

        /// <summary>
        /// Works the stats out from the games themselves rather than keeping running totals,
        /// so they can never drift from what was actually played.
        /// A streak is consecutive days won, like Wordle: a loss or a skipped day ends it, but
        /// a game still being played today doesn't end yesterday's streak.
        /// </summary>
        public static WordleStats CalculateStats(IEnumerable<WordleGameSummary> games, DateOnly today)
        {
            // a game from an earlier day that was left unfinished counts as a loss
            var counted = games
                .Where(x => x.GuessCount > 0 && (x.Solved || x.GuessCount >= MaxGuesses || x.Date < today))
                .OrderBy(x => x.Date)
                .ToList();

            var distribution = new int[MaxGuesses];
            var run = 0;
            var maxStreak = 0;
            DateOnly? previousDate = null;

            foreach (var game in counted)
            {
                if (game.Solved)
                {
                    run = previousDate == game.Date.AddDays(-1) ? run + 1 : 1;
                    distribution[Math.Clamp(game.GuessCount, 1, MaxGuesses) - 1]++;
                }
                else
                {
                    run = 0;
                }

                maxStreak = Math.Max(maxStreak, run);
                previousDate = game.Date;
            }

            // the streak is only still alive if the last win was today or yesterday
            var last = counted.LastOrDefault();
            var currentStreak = last != null && last.Solved && last.Date >= today.AddDays(-1) ? run : 0;

            return new WordleStats(counted.Count, counted.Count(x => x.Solved), currentStreak, maxStreak, distribution);
        }

        public static string RenderStats(WordleStats stats)
        {
            var text = new StringBuilder();
            text.AppendLine($"**Played** {stats.Played} · **Win %** {stats.WinPercentage} · **Streak** {stats.CurrentStreak} · **Best streak** {stats.MaxStreak}");

            var mostWins = stats.GuessDistribution.Max();

            for (var i = 0; i < stats.GuessDistribution.Length; i++)
            {
                var wins = stats.GuessDistribution[i];

                // scaled so the most common result is 8 squares, with at least one for any win
                var barLength = wins == 0 ? 0 : Math.Max(1, (int)Math.Round(wins * 8.0 / mostWins));
                var bar = wins == 0 ? "▫️" : string.Concat(Enumerable.Repeat("🟩", barLength));

                text.AppendLine($"`{i + 1}` {bar} {wins}");
            }

            return text.ToString().TrimEnd();
        }
    }
}
