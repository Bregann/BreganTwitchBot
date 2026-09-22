namespace BreganTwitchBot.Domain.Services.Helpers
{
    /// <summary>
    /// The Discord xp curve.
    ///
    /// This lives here so the levelling service and the /level command read from the same
    /// place - the old bot had the curve written out twice, so what /level showed could drift
    /// from what actually levelled somebody up.
    /// </summary>
    public static class DiscordLevelHelper
    {
        private const long BaseXp = 10;

        /// <summary>
        /// The total xp needed to reach the level after this one
        /// </summary>
        public static long GetXpNeededForNextLevel(int currentLevel)
        {
            return currentLevel switch
            {
                0 => 5,
                1 => 10,
                _ => (long)Math.Round(BaseXp * (currentLevel - 1) * 1.08 * currentLevel)
            };
        }

        /// <summary>
        /// The xp that was needed to reach the current level, so progress through the current
        /// level can be worked out rather than progress from zero
        /// </summary>
        public static long GetXpNeededForCurrentLevel(int currentLevel)
        {
            return currentLevel switch
            {
                0 => 0,
                1 => 5,
                2 => 10,
                _ => (long)Math.Round(BaseXp * (currentLevel - 2) * 1.08 * (currentLevel - 1))
            };
        }

        /// <summary>
        /// Scales the xp a message earns by the sender's level.
        ///
        /// The thresholds grow roughly with the square of the level, so flat xp meant each
        /// level took noticeably longer than the last and high levels stalled. This gives back
        /// a little of that, without making levels so fast that they stop meaning anything -
        /// the bonus is deliberately far slower than the curve it is offsetting.
        /// </summary>
        /// <param name="baseXp">What the message is worth before scaling</param>
        /// <param name="currentLevel">The sender's level</param>
        public static long ScaleXpForLevel(long baseXp, int currentLevel)
        {
            if (currentLevel <= 0)
            {
                return baseXp;
            }

            // +10% per level, capped at double. A level 30 sender earns twice what a level 1
            // sender does, rather than the 30x the threshold curve would otherwise demand
            var multiplier = Math.Min(1 + (currentLevel * 0.1), 2.0);

            return (long)Math.Round(baseXp * multiplier);
        }

        /// <summary>
        /// How far through the current level somebody is, as a percentage
        /// </summary>
        public static double GetProgressPercentage(int currentLevel, long currentXp)
        {
            var levelStart = GetXpNeededForCurrentLevel(currentLevel);
            var levelEnd = GetXpNeededForNextLevel(currentLevel);
            var span = levelEnd - levelStart;

            if (span <= 0)
            {
                return 0;
            }

            var progress = (currentXp - levelStart) / (double)span * 100;
            return Math.Clamp(progress, 0, 100);
        }
    }
}
