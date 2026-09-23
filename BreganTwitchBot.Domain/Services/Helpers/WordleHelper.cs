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
        /// Everyday words only, so nobody loses on something they've never heard of
        /// </summary>
        public static readonly IReadOnlyList<string> Answers =
        [
            "about", "above", "actor", "acute", "adopt", "adult", "after", "again", "agent", "agree",
            "ahead", "alarm", "album", "alert", "alike", "alive", "allow", "alone", "along", "alter",
            "amber", "among", "angel", "anger", "angle", "angry", "apple", "apply", "arena", "argue",
            "arise", "armor", "aside", "audio", "avoid", "award", "aware", "bacon", "badge", "baker",
            "basic", "beach", "beard", "beast", "begin", "being", "below", "bench", "berry", "birth",
            "black", "blade", "blame", "blank", "blast", "blend", "bless", "blind", "block", "bloom",
            "board", "boast", "bonus", "boost", "booth", "brain", "brave", "bread", "break", "brick",
            "bride", "brief", "bring", "broad", "brown", "brush", "build", "bunch", "burst", "cabin",
            "cable", "camel", "candy", "canoe", "cargo", "carry", "catch", "cause", "chain", "chair",
            "chalk", "charm", "chart", "chase", "cheap", "check", "cheek", "cheer", "chess", "chest",
            "chief", "child", "chill", "choir", "civic", "claim", "class", "clean", "clear", "clerk",
            "click", "cliff", "climb", "clock", "close", "cloud", "coach", "coast", "cocoa", "color",
            "coral", "couch", "count", "court", "cover", "crack", "craft", "crane", "crash", "crawl",
            "crazy", "cream", "crisp", "crowd", "crown", "crumb", "crust", "cycle", "daily", "dairy",
            "dance", "delay", "depth", "diary", "dirty", "dizzy", "dough", "draft", "drain", "drama",
            "dream", "dress", "drift", "drink", "drive", "eager", "eagle", "early", "earth", "eight",
            "elbow", "elder", "empty", "enjoy", "enter", "entry", "equal", "error", "event", "exact",
            "extra", "fable", "faint", "fairy", "faith", "false", "fancy", "feast", "fence", "ferry",
            "fever", "field", "fifty", "final", "flame", "flash", "fleet", "float", "flock", "flood",
            "floor", "flour", "fluid", "focus", "force", "forge", "forum", "found", "frame", "fresh",
            "front", "frost", "fruit", "funny", "ghost", "giant", "given", "glass", "globe", "glove",
            "grace", "grade", "grain", "grand", "grape", "grass", "great", "green", "greet", "grill",
            "group", "guard", "guess", "guest", "guide", "habit", "happy", "heart", "heavy", "hello",
            "hobby", "honey", "horse", "hotel", "house", "human", "humor", "ideal", "image", "index",
            "inner", "input", "issue", "jelly", "jewel", "joint", "judge", "juice", "knife", "knock",
            "label", "lemon", "level", "light", "limit", "linen", "lodge", "logic", "loyal", "lucky",
            "lunch", "magic", "major", "maple", "march", "match", "mayor", "medal", "melon", "mercy",
            "metal", "model", "money", "month", "moral", "motor", "mouse", "mouth", "movie", "music",
            "nerve", "never", "night", "noble", "noise", "north", "novel", "nurse", "ocean", "offer",
            "often", "olive", "onion", "opera", "orbit", "order", "other", "otter", "owner", "paint",
            "panel", "paper", "party", "pasta", "patch", "peace", "peach", "pearl", "pedal", "penny",
            "phone", "photo", "piano", "piece", "pilot", "pitch", "pizza", "place", "plain", "plane",
            "plant", "plate", "point", "polar", "porch", "pound", "power", "press", "price", "pride",
            "prize", "proof", "proud", "puppy", "queen", "quest", "quick", "quiet", "quilt", "quite",
            "radio", "raise", "rally", "ranch", "range", "rapid", "raven", "reach", "ready", "relax",
            "reply", "ridge", "right", "river", "roast", "robin", "robot", "rocky", "round", "route",
            "royal", "rugby", "ruler", "salad", "sauce", "scale", "scarf", "scene", "scout", "seven",
            "shade", "shape", "share", "shark", "sharp", "sheep", "shelf", "shell", "shine", "shirt",
            "shock", "shore", "short", "shout", "skate", "skill", "skirt", "skull", "sleep", "slice",
            "slide", "smart", "smile", "smoke", "snack", "snail", "snake", "solar", "solid", "sound",
            "south", "space", "spare", "spark", "speak", "speed", "spell", "spend", "spice", "spike",
            "spoon", "sport", "spray", "squad", "stack", "staff", "stage", "stair", "stamp", "stand",
            "start", "steam", "steel", "stick", "still", "stone", "storm", "story", "stove", "straw",
            "study", "style", "sugar", "sunny", "super", "swamp", "sweet", "swing", "sword", "table",
            "taste", "teach", "thank", "theme", "thick", "thing", "think", "three", "throw", "thumb",
            "tiger", "toast", "today", "tooth", "topic", "torch", "total", "touch", "tower", "track",
            "trade", "trail", "train", "treat", "trend", "trial", "tribe", "trick", "truck", "trust",
            "truth", "tulip", "twist", "uncle", "under", "unity", "upper", "upset", "urban", "usual",
            "valid", "value", "video", "visit", "vital", "vivid", "vocal", "voice", "wagon", "waste",
            "watch", "water", "whale", "wheat", "wheel", "white", "whole", "world", "worry", "wrist",
            "write", "young", "youth", "zebra"
        ];

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

        public static bool IsValidGuess(string guess)
        {
            return guess.Length == WordLength && guess.All(char.IsAsciiLetterLower);
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

        /// <summary>
        /// Letters that have been guessed and aren't anywhere in the word
        /// </summary>
        public static string GetRuledOutLetters(IEnumerable<string> guesses, string answer)
        {
            var ruledOut = guesses
                .SelectMany(x => x)
                .Where(x => !answer.Contains(x))
                .Distinct()
                .OrderBy(x => x)
                .Select(x => char.ToUpper(x));

            return string.Join(" ", ruledOut);
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
