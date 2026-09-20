namespace BreganTwitchBot.Domain.Services.Helpers
{
    /// <summary>
    /// Formats TimeSpans into the chat friendly wording the bot uses, eg
    /// "2 days, 3 hours, 1 minute and 5 seconds". Units that are zero are left out,
    /// so a short duration reads as "5 minutes and 2 seconds" rather than
    /// "0 years, 0 days, 0 hours, 5 minutes and 2 seconds".
    /// </summary>
    public static class DurationFormatHelper
    {
        public static string Humanise(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
            {
                timeSpan = TimeSpan.Zero;
            }

            var years = (int)(timeSpan.TotalDays / 365);
            var days = timeSpan.Days % 365;

            var parts = new List<string>();

            if (years > 0)
            {
                parts.Add(Pluralise(years, "year"));
            }

            if (days > 0)
            {
                parts.Add(Pluralise(days, "day"));
            }

            if (timeSpan.Hours > 0)
            {
                parts.Add(Pluralise(timeSpan.Hours, "hour"));
            }

            if (timeSpan.Minutes > 0)
            {
                parts.Add(Pluralise(timeSpan.Minutes, "minute"));
            }

            // always show seconds if nothing else would be shown, so we never return an empty string
            if (timeSpan.Seconds > 0 || parts.Count == 0)
            {
                parts.Add(Pluralise(timeSpan.Seconds, "second"));
            }

            if (parts.Count == 1)
            {
                return parts[0];
            }

            return string.Join(", ", parts[..^1]) + " and " + parts[^1];
        }

        private static string Pluralise(int value, string unit)
        {
            return $"{value} {unit}{(value == 1 ? "" : "s")}";
        }
    }
}
