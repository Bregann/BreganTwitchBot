using BreganTwitchBot.Domain.Services.Helpers;

namespace BreganTwitchBot.DomainTests.Discord
{
    [TestFixture]
    public class SeasonalNicknameHelperTests
    {
        [Test]
        public void GetSeason_MatchesByPrefix()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SeasonalNicknameHelper.GetSeason("christmas-tree"), Is.SameAs(SeasonalNicknameHelper.ChristmasEmojis));
                Assert.That(SeasonalNicknameHelper.GetSeason("spooky-bat"), Is.SameAs(SeasonalNicknameHelper.SpookyEmojis));
                Assert.That(SeasonalNicknameHelper.GetSeason("giveaway-abc-enter"), Is.Null);
            });
        }

        [Test]
        public void GetEmoji_UnknownButtonInASeason_IsNull()
        {
            Assert.That(SeasonalNicknameHelper.GetEmoji(SeasonalNicknameHelper.SpookyEmojis, "spooky-notathing"), Is.Null);
        }

        [Test]
        public void AddEmoji_WrapsTheName()
        {
            Assert.That(SeasonalNicknameHelper.AddEmoji("aly", "🦇"), Is.EqualTo("🦇aly🦇"));
        }

        [Test]
        public void RemoveEmojis_StripsEverySpookyEmoji()
        {
            var name = "aly";

            foreach (var (_, emoji) in SeasonalNicknameHelper.SpookyEmojis)
            {
                name = SeasonalNicknameHelper.AddEmoji(name, emoji);
            }

            Assert.That(SeasonalNicknameHelper.RemoveEmojis(name, SeasonalNicknameHelper.SpookyEmojis), Is.EqualTo("aly"));
        }

        [Test]
        public void RemoveEmojis_LeavesOtherSeasonsAlone()
        {
            // unspooking in december shouldn't take somebody's christmas tree off too
            var name = "🎄🎃aly🎃🎄";

            Assert.That(SeasonalNicknameHelper.RemoveEmojis(name, SeasonalNicknameHelper.SpookyEmojis), Is.EqualTo("🎄aly🎄"));
        }

        [Test]
        public void Seasons_HaveUniqueButtonsAndEmojis()
        {
            foreach (var season in new[] { SeasonalNicknameHelper.ChristmasEmojis, SeasonalNicknameHelper.SpookyEmojis })
            {
                Assert.Multiple(() =>
                {
                    Assert.That(season.Select(x => x.ButtonId), Is.Unique);
                    Assert.That(season.Select(x => x.Emoji), Is.Unique);
                });
            }
        }

        [Test]
        public void Seasons_FitInDiscordsButtonLimit()
        {
            // discord allows 25 buttons a message, and each season also gets a reset button
            Assert.Multiple(() =>
            {
                Assert.That(SeasonalNicknameHelper.ChristmasEmojis, Has.Count.LessThan(25));
                Assert.That(SeasonalNicknameHelper.SpookyEmojis, Has.Count.LessThan(25));
            });
        }
    }
}
