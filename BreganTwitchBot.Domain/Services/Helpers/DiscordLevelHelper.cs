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
