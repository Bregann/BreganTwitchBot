namespace BreganTwitchBot.Domain.Services.Helpers
{
    /// <summary>
    /// The emoji a user can wrap their Discord nickname in for /christmas and /spook.
    /// Button ids are "{season}-{name}", and "{season}-resetusername" strips that season's emojis.
    /// </summary>
    public static class SeasonalNicknameHelper
    {
        public const string ChristmasPrefix = "christmas-";
        public const string SpookyPrefix = "spooky-";

        // only emoji that display as emoji by default, so none need a variation selector
        // that Replace could miss when the name is reset
        public static readonly IReadOnlyList<(string ButtonId, string Emoji)> ChristmasEmojis =
        [
            ("christmas-snowman", "⛄"),
            ("christmas-gift", "🎁"),
            ("christmas-tree", "🎄"),
            ("christmas-santa", "🎅"),
            ("christmas-mrsanta", "🤶"),
            ("christmas-star", "🌟"),
            ("christmas-socks", "🧦"),
            ("christmas-bell", "🔔"),
            ("christmas-deer", "🦌")
        ];

        public static readonly IReadOnlyList<(string ButtonId, string Emoji)> SpookyEmojis =
        [
            ("spooky-pumpkin", "🎃"),
            ("spooky-ghost", "👻"),
            ("spooky-skull", "💀"),
            ("spooky-bat", "🦇"),
            ("spooky-vampire", "🧛"),
            ("spooky-zombie", "🧟"),
            ("spooky-witch", "🧙"),
            ("spooky-candy", "🍬"),
            ("spooky-crystalball", "🔮"),
            ("spooky-moon", "🌕")
        ];

        /// <summary>
        /// Gets the season a button belongs to, or null if it isn't a seasonal nickname button
        /// </summary>
        public static IReadOnlyList<(string ButtonId, string Emoji)>? GetSeason(string buttonId)
        {
            if (buttonId.StartsWith(ChristmasPrefix))
            {
                return ChristmasEmojis;
            }

            if (buttonId.StartsWith(SpookyPrefix))
            {
                return SpookyEmojis;
            }

            return null;
        }

        public static bool IsReset(string buttonId)
        {
            return buttonId.EndsWith("-resetusername");
        }

        /// <summary>
        /// Gets the emoji for a button, or null if the season has no such button
        /// </summary>
        public static string? GetEmoji(IReadOnlyList<(string ButtonId, string Emoji)> season, string buttonId)
        {
            return season.FirstOrDefault(x => x.ButtonId == buttonId).Emoji;
        }

        public static string AddEmoji(string name, string emoji)
        {
            return emoji + name + emoji;
        }

        public static string RemoveEmojis(string name, IReadOnlyList<(string ButtonId, string Emoji)> season)
        {
            foreach (var (_, emoji) in season)
            {
                name = name.Replace(emoji, "");
            }

            return name.Trim();
        }
    }
}
