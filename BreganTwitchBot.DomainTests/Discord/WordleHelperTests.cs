using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using static BreganTwitchBot.Domain.Services.Helpers.WordleLetterResult;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class WordleHelperTests
    {
        [Test]
        public void Answers_AreAllFiveLowercaseLettersAndUnique()
        {
            Assert.Multiple(() =>
            {
                Assert.That(WordleHelper.Answers.All(WordleHelper.IsValidGuess), Is.True);
                Assert.That(WordleHelper.Answers, Is.Unique);
            });
        }

        [Test]
        public void Score_ExactMatch_IsAllCorrect()
        {
            Assert.That(WordleHelper.Score("crane", "crane"), Is.EqualTo(new[] { Correct, Correct, Correct, Correct, Correct }));
        }

        [Test]
        public void Score_MarksCorrectPresentAndAbsent()
        {
            // c is right, r is in the word elsewhere, the rest aren't in "cider"
            Assert.That(WordleHelper.Score("crabs", "cider"), Is.EqualTo(new[] { Correct, Present, Absent, Absent, Absent }));
        }

        [Test]
        public void Score_RepeatedGuessLetter_OnlyMarksAsManyAsTheAnswerHas()
        {
            // "eagle" has two e's: the last one is exact, so only one more e can be present
            Assert.That(WordleHelper.Score("geese", "eagle"), Is.EqualTo(new[] { Present, Present, Absent, Absent, Correct }));
        }

        [Test]
        public void Score_ExactMatchTakesPriorityOverAnEarlierPresent()
        {
            // the second l in "hello" is exact, so the first l can only be present if "world" had two
            Assert.That(WordleHelper.Score("hello", "world"), Is.EqualTo(new[] { Absent, Absent, Absent, Correct, Present }));
        }

        [Test]
        public void GetWordForDate_IsTheSameForTheSameDay()
        {
            var date = new DateOnly(2026, 10, 31);
            Assert.That(WordleHelper.GetWordForDate(date), Is.EqualTo(WordleHelper.GetWordForDate(date)));
        }

        [Test]
        public void GetWordForDate_DoesNotRepeatUntilEveryWordHasBeenUsed()
        {
            var start = new DateOnly(2026, 1, 1);
            var words = Enumerable.Range(0, WordleHelper.Answers.Count)
                .Select(x => WordleHelper.GetWordForDate(start.AddDays(x)))
                .ToList();

            Assert.That(words, Is.Unique);
        }

        [TestCase("crane", true)]
        [TestCase("cran", false)]
        [TestCase("cranes", false)]
        [TestCase("cr4ne", false)]
        [TestCase("cr ne", false)]
        public void IsValidGuess(string guess, bool expected)
        {
            Assert.That(WordleHelper.IsValidGuess(guess), Is.EqualTo(expected));
        }

        [Test]
        public void GetPointsForGuesses_FewerGuessesEarnMore()
        {
            for (var guesses = 1; guesses < WordleHelper.MaxGuesses; guesses++)
            {
                Assert.That(WordleHelper.GetPointsForGuesses(guesses), Is.GreaterThan(WordleHelper.GetPointsForGuesses(guesses + 1)));
            }

            Assert.That(WordleHelper.GetPointsForGuesses(WordleHelper.MaxGuesses + 1), Is.Zero);
        }

        [Test]
        public void RenderRow_WithoutLetters_GivesNothingAway()
        {
            Assert.That(WordleHelper.RenderRow("crabs", "cider", showLetters: false), Is.EqualTo("🟩🟨⬛⬛⬛"));
        }

        [Test]
        public void GetRuledOutLetters_ListsGuessedLettersNotInTheWord()
        {
            Assert.That(WordleHelper.GetRuledOutLetters(["crabs", "about"], "cider"), Is.EqualTo("A B O S T U"));
        }

        // stats and streaks

        private static readonly DateOnly Today = new(2026, 9, 23);

        private static WordleGameSummary Won(int daysAgo, int guesses = 3) => new(Today.AddDays(-daysAgo), guesses, true);
        private static WordleGameSummary Lost(int daysAgo) => new(Today.AddDays(-daysAgo), WordleHelper.MaxGuesses, false);

        [Test]
        public void Stats_NoGames_AreAllZero()
        {
            var stats = WordleHelper.CalculateStats([], Today);

            Assert.Multiple(() =>
            {
                Assert.That(stats.Played, Is.Zero);
                Assert.That(stats.WinPercentage, Is.Zero);
                Assert.That(stats.CurrentStreak, Is.Zero);
                Assert.That(stats.GuessDistribution, Is.EqualTo(new int[6]));
            });
        }

        [Test]
        public void Stats_ConsecutiveWins_BuildAStreak()
        {
            var stats = WordleHelper.CalculateStats([Won(2), Won(1), Won(0)], Today);

            Assert.Multiple(() =>
            {
                Assert.That(stats.CurrentStreak, Is.EqualTo(3));
                Assert.That(stats.MaxStreak, Is.EqualTo(3));
            });
        }

        [Test]
        public void Stats_ALoss_ResetsTheStreakButKeepsTheBest()
        {
            var stats = WordleHelper.CalculateStats([Won(3), Won(2), Lost(1), Won(0)], Today);

            Assert.Multiple(() =>
            {
                Assert.That(stats.CurrentStreak, Is.EqualTo(1));
                Assert.That(stats.MaxStreak, Is.EqualTo(2));
            });
        }

        [Test]
        public void Stats_ASkippedDay_ResetsTheStreak()
        {
            var stats = WordleHelper.CalculateStats([Won(4), Won(3), Won(1), Won(0)], Today);

            Assert.That(stats.CurrentStreak, Is.EqualTo(2));
        }

        [Test]
        public void Stats_NotPlayedYetToday_KeepsYesterdaysStreak()
        {
            var stats = WordleHelper.CalculateStats([Won(2), Won(1)], Today);

            Assert.That(stats.CurrentStreak, Is.EqualTo(2));
        }

        [Test]
        public void Stats_TodayInProgress_DoesNotCountOrBreakTheStreak()
        {
            var stats = WordleHelper.CalculateStats([Won(1), new WordleGameSummary(Today, 2, false)], Today);

            Assert.Multiple(() =>
            {
                Assert.That(stats.Played, Is.EqualTo(1));
                Assert.That(stats.CurrentStreak, Is.EqualTo(1));
            });
        }

        [Test]
        public void Stats_LastWinTwoDaysAgo_HasNoCurrentStreak()
        {
            var stats = WordleHelper.CalculateStats([Won(3), Won(2)], Today);

            Assert.Multiple(() =>
            {
                Assert.That(stats.CurrentStreak, Is.Zero);
                Assert.That(stats.MaxStreak, Is.EqualTo(2));
            });
        }

        [Test]
        public void Stats_AnAbandonedEarlierGame_CountsAsALoss()
        {
            var stats = WordleHelper.CalculateStats([Won(2), new WordleGameSummary(Today.AddDays(-1), 2, false), Won(0)], Today);

            Assert.Multiple(() =>
            {
                Assert.That(stats.Played, Is.EqualTo(3));
                Assert.That(stats.Won, Is.EqualTo(2));
                Assert.That(stats.CurrentStreak, Is.EqualTo(1));
            });
        }

        [Test]
        public void Stats_CountWinsByGuesses()
        {
            var stats = WordleHelper.CalculateStats([Won(4, 1), Won(3, 3), Won(2, 3), Lost(1), Won(0, 6)], Today);

            Assert.Multiple(() =>
            {
                Assert.That(stats.GuessDistribution, Is.EqualTo(new[] { 1, 0, 2, 0, 0, 1 }));
                Assert.That(stats.Played, Is.EqualTo(5));
                Assert.That(stats.WinPercentage, Is.EqualTo(80));
            });
        }

        [Test]
        public void RenderStats_ShowsEveryGuessCount()
        {
            var text = WordleHelper.RenderStats(WordleHelper.CalculateStats([Won(1, 2), Won(0, 4)], Today));

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("**Streak** 2"));
                for (var guesses = 1; guesses <= WordleHelper.MaxGuesses; guesses++)
                {
                    Assert.That(text, Does.Contain($"`{guesses}`"));
                }
            });
        }
    }
}
