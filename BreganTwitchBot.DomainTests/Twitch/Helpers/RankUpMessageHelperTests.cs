using BreganTwitchBot.Domain.Services.Helpers;

namespace BreganTwitchBot.DomainTests.Twitch.Helpers
{
    [TestFixture]
    public class RankUpMessageHelperTests
    {
        [Test]
        public void OnePerson_ReadsLikeItAlwaysHas()
        {
            var message = RankUpMessageHelper.BuildMessage("Gold", 600, ["alice"], 0, false, 0);

            Assert.That(message, Is.EqualTo("Congrats @alice, you earned the Gold rank by watching 600 minutes in the stream! Keep watching to earn a higher rank!"));
        }

        [Test]
        public void TwoPeople_AreJoinedWithAnd()
        {
            var message = RankUpMessageHelper.BuildMessage("Gold", 600, ["alice", "bob"], 0, false, 0);

            Assert.That(message, Does.StartWith("Congrats @alice and @bob, you earned the Gold rank"));
        }

        [Test]
        public void ThreePeople_AreListed()
        {
            var message = RankUpMessageHelper.BuildMessage("Gold", 600, ["alice", "bob", "carol"], 0, false, 0);

            Assert.That(message, Does.StartWith("Congrats @alice, @bob and @carol, you earned the Gold rank"));
        }

        [TestCase(1, "1 other person also earned it!")]
        [TestCase(4, "4 other people also earned it!")]
        public void EveryoneElse_IsCountedNotNamed(int others, string expected)
        {
            var message = RankUpMessageHelper.BuildMessage("Gold", 600, ["alice", "bob", "carol"], others, false, 0);

            Assert.That(message, Does.Contain($"in the stream! {expected} Keep watching"));
        }

        [Test]
        public void LargeMinutes_HaveSeparators()
        {
            Assert.That(RankUpMessageHelper.BuildMessage("Diamond", 12000, ["alice"], 0, false, 0), Does.Contain("12,000 minutes"));
        }

        [TestCase(1, 0, "Make sure to join the Discord and link your Twitch account to unlock your rank role!")]
        [TestCase(1, 1, "Your rank has been applied in the Discord")]
        [TestCase(3, 3, "Your ranks have been applied in the Discord")]
        [TestCase(3, 1, "Linked Discord accounts have got the rank role, link your Twitch account in the Discord to unlock yours!")]
        public void DiscordText_DependsOnWhoIsLinked(int people, int linked, string expected)
        {
            var named = new[] { "alice", "bob", "carol" }.Take(people).ToList();
            var message = RankUpMessageHelper.BuildMessage("Gold", 600, named, 0, true, linked);

            Assert.That(message, Does.EndWith(expected));
        }
    }
}
