using BreganTwitchBot.Domain.Services.Helpers;

namespace BreganTwitchBot.DomainTests.Discord
{
    /// <summary>
    /// The curve was written out twice in the old bot, so what /level showed could drift from
    /// what actually levelled somebody up. These pin the shared version.
    /// </summary>
    [TestFixture]
    public class DiscordLevelHelperTests
    {
        [Test]
        public void XpForNextLevel_MatchesTheOriginalCurve()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DiscordLevelHelper.GetXpNeededForNextLevel(0), Is.EqualTo(5));
                Assert.That(DiscordLevelHelper.GetXpNeededForNextLevel(1), Is.EqualTo(10));

                // level 2 and up: baseXp(10) * (level - 1) * 1.08 * level
                Assert.That(DiscordLevelHelper.GetXpNeededForNextLevel(2), Is.EqualTo(22));
                Assert.That(DiscordLevelHelper.GetXpNeededForNextLevel(5), Is.EqualTo(216));
            });
        }

        [Test]
        public void XpForNextLevel_AlwaysIncreases()
        {
            for (var level = 2; level < 50; level++)
            {
                Assert.That(
                    DiscordLevelHelper.GetXpNeededForNextLevel(level),
                    Is.GreaterThan(DiscordLevelHelper.GetXpNeededForNextLevel(level - 1)),
                    $"level {level} should need more xp than {level - 1}");
            }
        }

        [Test]
        public void XpForCurrentLevel_IsBelowTheNextThreshold()
        {
            for (var level = 0; level < 20; level++)
            {
                Assert.That(
                    DiscordLevelHelper.GetXpNeededForCurrentLevel(level),
                    Is.LessThanOrEqualTo(DiscordLevelHelper.GetXpNeededForNextLevel(level)),
                    $"level {level} start should not be past its end");
            }
        }

        [Test]
        public void Progress_AtTheStartOfALevel_IsZero()
        {
            var levelStart = DiscordLevelHelper.GetXpNeededForCurrentLevel(5);

            Assert.That(DiscordLevelHelper.GetProgressPercentage(5, levelStart), Is.EqualTo(0).Within(0.01));
        }

        [Test]
        public void Progress_AtTheNextThreshold_IsOneHundred()
        {
            var levelEnd = DiscordLevelHelper.GetXpNeededForNextLevel(5);

            Assert.That(DiscordLevelHelper.GetProgressPercentage(5, levelEnd), Is.EqualTo(100).Within(0.01));
        }

        [Test]
        public void Progress_IsClampedBetweenZeroAndOneHundred()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DiscordLevelHelper.GetProgressPercentage(5, 0), Is.EqualTo(0));
                Assert.That(DiscordLevelHelper.GetProgressPercentage(5, 999999), Is.EqualTo(100));
            });
        }

        // xp scaling - #19

        [Test]
        public void ScaleXp_AtLevelZero_IsUnchanged()
        {
            Assert.That(DiscordLevelHelper.ScaleXpForLevel(5, 0), Is.EqualTo(5));
        }

        [Test]
        public void ScaleXp_IncreasesWithLevel()
        {
            var atLevelOne = DiscordLevelHelper.ScaleXpForLevel(10, 1);
            var atLevelFive = DiscordLevelHelper.ScaleXpForLevel(10, 5);

            Assert.That(atLevelFive, Is.GreaterThan(atLevelOne));
        }

        [Test]
        public void ScaleXp_IsCappedAtDouble()
        {
            // without a cap a very high level would earn absurd xp per message
            Assert.Multiple(() =>
            {
                Assert.That(DiscordLevelHelper.ScaleXpForLevel(10, 50), Is.EqualTo(20));
                Assert.That(DiscordLevelHelper.ScaleXpForLevel(10, 500), Is.EqualTo(20));
            });
        }

        [Test]
        public void ScaleXp_NeverReducesTheBaseAmount()
        {
            for (var level = 0; level < 60; level++)
            {
                Assert.That(
                    DiscordLevelHelper.ScaleXpForLevel(5, level),
                    Is.GreaterThanOrEqualTo(5),
                    $"level {level} should never earn less than the base amount");
            }
        }

        [Test]
        public void ScaleXp_GrowsFarSlowerThanTheThresholds()
        {
            // the bonus offsets some of the curve, it does not overtake it - otherwise
            // levelling would get faster the further up somebody went
            var thresholdGrowth = DiscordLevelHelper.GetXpNeededForNextLevel(10) / (double)DiscordLevelHelper.GetXpNeededForNextLevel(2);
            var xpGrowth = DiscordLevelHelper.ScaleXpForLevel(10, 10) / (double)DiscordLevelHelper.ScaleXpForLevel(10, 2);

            Assert.That(xpGrowth, Is.LessThan(thresholdGrowth));
        }
    }
}
