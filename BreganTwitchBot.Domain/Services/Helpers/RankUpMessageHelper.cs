namespace BreganTwitchBot.Domain.Services.Helpers
{
    public static class RankUpMessageHelper
    {
        /// <summary>
        /// The most people one rank up message names. Anyone else who earned the rank is counted instead
        /// </summary>
        public const int MaxNamedUsers = 3;

        /// <summary>
        /// One message for everyone who earned the same rank at the same time, instead of one each
        /// </summary>
        /// <param name="namedUsers">Who to congratulate by name, up to <see cref="MaxNamedUsers"/></param>
        /// <param name="otherUsers">Everyone else who earned it, who's counted but not pinged</param>
        /// <param name="linkedUsers">How many of all of them have their Discord linked, so got the role</param>
        public static string BuildMessage(string rankName, int minutesRequired, IReadOnlyList<string> namedUsers, int otherUsers, bool discordEnabled, int linkedUsers)
        {
            var totalUsers = namedUsers.Count + otherUsers;
            var names = JoinNames(namedUsers.Select(x => $"@{x}").ToList());

            var message = $"Congrats {names}, you earned the {rankName} rank by watching {minutesRequired:N0} minutes in the stream!";

            if (otherUsers > 0)
            {
                message += otherUsers == 1 ? " 1 other person also earned it!" : $" {otherUsers:N0} other people also earned it!";
            }

            return $"{message} {GetDiscordText(discordEnabled, linkedUsers, totalUsers)}";
        }

        private static string GetDiscordText(bool discordEnabled, int linkedUsers, int totalUsers)
        {
            if (!discordEnabled)
            {
                return "Keep watching to earn a higher rank!";
            }

            if (linkedUsers == 0)
            {
                return "Make sure to join the Discord and link your Twitch account to unlock your rank role!";
            }

            if (linkedUsers == totalUsers)
            {
                return totalUsers == 1 ? "Your rank has been applied in the Discord" : "Your ranks have been applied in the Discord";
            }

            return "Linked Discord accounts have got the rank role, link your Twitch account in the Discord to unlock yours!";
        }

        /// <summary>
        /// "a", "a and b", "a, b and c"
        /// </summary>
        private static string JoinNames(IReadOnlyList<string> names)
        {
            return names.Count <= 1
                ? string.Join("", names)
                : $"{string.Join(", ", names.Take(names.Count - 1))} and {names[^1]}";
        }
    }
}
