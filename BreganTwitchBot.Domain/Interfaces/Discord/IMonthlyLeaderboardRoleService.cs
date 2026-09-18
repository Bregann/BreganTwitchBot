namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IMonthlyLeaderboardRoleService
    {
        /// <summary>
        /// Moves the monthly bits and gifted subs leaderboard roles onto whoever currently leads.
        /// On the first of the month the monthly totals are reset and the roles are cleared.
        /// </summary>
        Task UpdateMonthlyLeaderboardRolesAsync();
    }
}
