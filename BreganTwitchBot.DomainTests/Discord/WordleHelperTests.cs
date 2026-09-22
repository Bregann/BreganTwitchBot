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
    }
}
